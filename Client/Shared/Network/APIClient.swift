//
//  APIClient.swift
//  Client
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Foundation

final class APIClient: NSObject, URLSessionDelegate {
    static let shared = APIClient()

    private lazy var session: URLSession = {
        URLSession(configuration: .default, delegate: self, delegateQueue: nil)
    }()

    private let decoder: JSONDecoder = {
        let d = JSONDecoder()
        let long  = ISO8601DateFormatter()
        long.formatOptions  = [.withInternetDateTime, .withFractionalSeconds]
        let short = ISO8601DateFormatter()
        short.formatOptions = [.withInternetDateTime]
        d.dateDecodingStrategy = .custom { dec in
            let c   = try dec.singleValueContainer()
            let str = try c.decode(String.self)
            if let date = long.date(from: str)  { return date }
            if let date = short.date(from: str) { return date }
            throw DecodingError.dataCorruptedError(in: c, debugDescription: "Bad date: \(str)")
        }
        return d
    }()

    private let encoder: JSONEncoder = {
        let e = JSONEncoder()
        let f = ISO8601DateFormatter()
        f.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
        e.dateEncodingStrategy = .custom { date, enc in
            var c = enc.singleValueContainer()
            try c.encode(f.string(from: date))
        }
        return e
    }()

    func get<T: Decodable>(_ url: String) async throws -> T {
        try await perform(make(url, "GET"))
    }
    func post<B: Encodable, R: Decodable>(_ url: String, body: B) async throws -> R {
        var r = make(url, "POST"); r.httpBody = try encoder.encode(body)
        return try await perform(r)
    }
    func postVoid<B: Encodable>(_ url: String, body: B) async throws {
        var r = make(url, "POST"); r.httpBody = try encoder.encode(body)
        try await performVoid(r)
    }
    func postEmpty(_ url: String) async throws { try await performVoid(make(url, "POST")) }
    func put<B: Encodable, R: Decodable>(_ url: String, body: B) async throws -> R {
        var r = make(url, "PUT"); r.httpBody = try encoder.encode(body)
        return try await perform(r)
    }
    func putVoid(_ url: String) async throws { try await performVoid(make(url, "PUT")) }
    func delete(_ url: String) async throws  { try await performVoid(make(url, "DELETE")) }

    private func make(_ urlString: String, _ method: String) -> URLRequest {
        var r = URLRequest(url: URL(string: urlString)!)
        r.httpMethod = method
        r.setValue("application/json", forHTTPHeaderField: "Content-Type")
        r.setValue("application/json", forHTTPHeaderField: "Accept")
        return r
    }
    private func perform<T: Decodable>(_ req: URLRequest) async throws -> T {
        let (data, res) = try await session.data(for: req)
        try validate(res, data)
        do { return try decoder.decode(T.self, from: data) }
        catch { throw NetworkError.decodingFailed(error) }
    }
    private func performVoid(_ req: URLRequest) async throws {
        let (data, res) = try await session.data(for: req)
        try validate(res, data)
    }
    private func validate(_ res: URLResponse, _ data: Data) throws {
        guard let h = res as? HTTPURLResponse else { return }
        guard (200...299).contains(h.statusCode) else {
            throw NetworkError.serverError(h.statusCode, String(data: data, encoding: .utf8))
        }
    }
    func urlSession(_ session: URLSession,
                    didReceive challenge: URLAuthenticationChallenge,
                    completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void) {
        if challenge.protectionSpace.host == "localhost",
           let trust = challenge.protectionSpace.serverTrust {
            completionHandler(.useCredential, URLCredential(trust: trust))
        } else { completionHandler(.performDefaultHandling, nil) }
    }
}
