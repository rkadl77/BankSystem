import Foundation

struct TokenResponse: Decodable {
    let token: String
    let expiration: Date?

    init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        token = try container.decode(String.self, forKey: .token)
        expiration = try? container.decodeIfPresent(Date.self, forKey: .expiration)
    }

    private enum CodingKeys: String, CodingKey { case token, expiration }
}

final class AuthService {
    static let shared = AuthService()

    private struct LoginRequest: Encodable {
        let username: String
        let password: String
    }

    private lazy var session: URLSession = {
        URLSession(configuration: .default, delegate: LocalhostDelegate.shared, delegateQueue: nil)
    }()

    func login(username: String, password: String) async throws -> String {
        guard let url = URL(string: "\(API.auth)/Token") else { throw NetworkError.invalidURL }
        var req = URLRequest(url: url)
        req.httpMethod = "POST"
        req.setValue("application/json", forHTTPHeaderField: "Content-Type")
        req.httpBody = try JSONEncoder().encode(LoginRequest(username: username, password: password))

        let (data, response) = try await session.data(for: req)
        guard let http = response as? HTTPURLResponse else { throw NetworkError.unknown(URLError(.badServerResponse)) }
        guard (200...299).contains(http.statusCode) else {
            throw NetworkError.serverError(http.statusCode, String(data: data, encoding: .utf8))
        }

        if let parsed = try? JSONDecoder().decode(TokenResponse.self, from: data) {
            return parsed.token
        }
        if let raw = String(data: data, encoding: .utf8) {
            return raw.trimmingCharacters(in: CharacterSet(charactersIn: "\" \n\r"))
        }
        throw NetworkError.decodingFailed(URLError(.cannotParseResponse))
    }
}

final class LocalhostDelegate: NSObject, URLSessionDelegate {
    static let shared = LocalhostDelegate()
    func urlSession(_ session: URLSession,
                    didReceive challenge: URLAuthenticationChallenge,
                    completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        if challenge.protectionSpace.host == "localhost",
           let trust = challenge.protectionSpace.serverTrust {
            completionHandler(.useCredential, URLCredential(trust: trust))
        } else {
            completionHandler(.performDefaultHandling, nil)
        }
    }
}
