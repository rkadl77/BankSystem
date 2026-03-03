//
//  NetworkError.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

enum NetworkError: LocalizedError {
    case invalidURL
    case decodingFailed(Error)
    case serverError(Int, String?)
    case unknown(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:               return "Неверный URL"
        case .decodingFailed(let e):    return "Ошибка декодирования: \(e.localizedDescription)"
        case .serverError(let c, let m): return m.flatMap { $0.isEmpty ? nil : $0 } ?? "Ошибка сервера (\(c))"
        case .unknown(let e):           return e.localizedDescription
        }
    }
}
