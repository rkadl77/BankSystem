import Foundation

enum NetworkError: LocalizedError {
    case invalidURL
    case unauthorized
    case decodingFailed(Error)
    case serverError(Int, String?)
    case unknown(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:                return "Неверный URL"
        case .unauthorized:              return "Не авторизован. Войдите снова."
        case .decodingFailed(let e):     return "Ошибка декодирования: \(e.localizedDescription)"
        case .serverError(let c, let m): return m.flatMap { $0.isEmpty ? nil : $0 } ?? "Ошибка сервера (\(c))"
        case .unknown(let e):            return e.localizedDescription
        }
    }
}
