import Foundation
import Combine
import UIKit
import AuthenticationServices

@MainActor
final class AuthViewModel: ObservableObject {
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var currentUser: UserDTO?

    var isLoggedIn: Bool { currentUser != nil }

    private let callbackScheme = "bankapp-client"
    private let contextProvider = WebAuthContextProvider()
    private var webAuthSession: ASWebAuthenticationSession?

    private var authWebBase: String {
        guard let url = URL(string: API.auth),
              let scheme = url.scheme, let host = url.host else {
            return "http://localhost:5109"
        }
        let port = url.port.map { ":\($0)" } ?? ""
        return "\(scheme)://\(host)\(port)"
    }

    init() {
        if TokenStore.shared.token != nil, let userId = TokenStore.shared.userId {
            Task { await restoreSession(userId: userId) }
        }
    }

    func startWebLogin() { openAuthPage("login.html") }
    func startWebRegister() { openAuthPage("register.html") }

    func logout() {
        TokenStore.shared.token    = nil
        TokenStore.shared.userId   = nil
        TokenStore.shared.userRole = nil
        currentUser = nil
    }

    private func openAuthPage(_ path: String) {
        errorMessage = nil
        let redirect = "\(callbackScheme)://auth/callback"
        guard
            var components = URLComponents(string: "\(authWebBase)/\(path)")
        else { return }
        components.queryItems = [URLQueryItem(name: "redirect_uri", value: redirect)]
        guard let url = components.url else { return }

        let session = ASWebAuthenticationSession(
            url: url,
            callbackURLScheme: callbackScheme
        ) { [weak self] callbackURL, error in
            Task { @MainActor [weak self] in
                await self?.handleCallback(url: callbackURL, error: error)
            }
        }
        session.presentationContextProvider = contextProvider
        session.prefersEphemeralWebBrowserSession = false
        webAuthSession = session
        session.start()
    }

    private func handleCallback(url: URL?, error: Error?) async {
        if let err = error as? ASWebAuthenticationSessionError,
           err.code == .canceledLogin { return }
        guard error == nil, let url else {
            errorMessage = error?.localizedDescription; return
        }
        guard
            let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
            let token = components.queryItems?.first(where: { $0.name == "token" })?.value
        else {
            errorMessage = "Сервис авторизации не вернул токен"; return
        }

        isLoading = true
        defer { isLoading = false }
        TokenStore.shared.token = token

        do {
            let user = try await resolveUser(from: token, urlComponents: components)
            guard user.isActive else {
                errorMessage = "Аккаунт заблокирован"
                TokenStore.shared.token = nil; return
            }
            guard user.role == "client" else {
                errorMessage = "Нет доступа как клиент"
                TokenStore.shared.token = nil; return
            }
            TokenStore.shared.userId   = user.id
            TokenStore.shared.userRole = user.role
            currentUser = user
        } catch {
            TokenStore.shared.token = nil
            errorMessage = error.localizedDescription
        }
    }

    private func restoreSession(userId: UUID) async {
        do {
            let user = try await UserService.shared.getUser(id: userId)
            guard user.isActive, user.role == "client" else {
                TokenStore.shared.token = nil; return
            }
            currentUser = user
        } catch { TokenStore.shared.token = nil }
    }

    private func resolveUser(from token: String, urlComponents: URLComponents) async throws -> UserDTO {
        if let raw = urlComponents.queryItems?.first(where: { $0.name == "userId" })?.value,
           let id = UUID(uuidString: raw) {
            return try await UserService.shared.getUser(id: id)
        }
        let claims = decodeJWTPayload(token)
        if let sub = claims["sub"] as? String, let id = UUID(uuidString: sub) {
            return try await UserService.shared.getUser(id: id)
        }
        if let email = (claims["email"] as? String) ?? (claims["username"] as? String) {
            return try await UserService.shared.getUserByEmail(email)
        }
        throw NetworkError.serverError(0, "Невозможно идентифицировать пользователя по токену")
    }

    private func decodeJWTPayload(_ token: String) -> [String: Any] {
        let parts = token.split(separator: ".")
        guard parts.count == 3 else { return [:] }
        var base64 = String(parts[1])
        let pad = base64.count % 4
        if pad != 0 { base64 += String(repeating: "=", count: 4 - pad) }
        base64 = base64
            .replacingOccurrences(of: "-", with: "+")
            .replacingOccurrences(of: "_", with: "/")
        guard let data = Data(base64Encoded: base64),
              let json = try? JSONSerialization.jsonObject(with: data) as? [String: Any]
        else { return [:] }
        return json
    }
}

final class WebAuthContextProvider: NSObject, ASWebAuthenticationPresentationContextProviding {
    func presentationAnchor(for session: ASWebAuthenticationSession) -> ASPresentationAnchor {
        UIApplication.shared.connectedScenes
            .compactMap { $0 as? UIWindowScene }
            .flatMap { $0.windows }
            .first(where: { $0.isKeyWindow }) ?? ASPresentationAnchor()
    }
}
