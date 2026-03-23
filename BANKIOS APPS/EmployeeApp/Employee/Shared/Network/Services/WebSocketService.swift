import Foundation
import Combine

@MainActor
final class TransactionWebSocketService: NSObject {
    private var task: URLSessionWebSocketTask?
    private var urlSession: URLSession?
    private var onInvalidate: (() -> Void)?

    func connect(accountId: UUID, onInvalidate: @escaping () -> Void) {
        self.onInvalidate = onInvalidate
        disconnect()

        let urlString = "ws://localhost:5116/api/ws/account/\(accountId)"
        guard let url = URL(string: urlString) else { return }

        let config = URLSessionConfiguration.default
        urlSession = URLSession(configuration: config, delegate: self, delegateQueue: .main)
        task = urlSession?.webSocketTask(with: url)
        task?.resume()
        receiveLoop()
    }

    func disconnect() {
        task?.cancel(with: .normalClosure, reason: nil)
        task = nil
        urlSession?.invalidateAndCancel()
        urlSession = nil
    }

    private func receiveLoop() {
        task?.receive { [weak self] result in
            Task { @MainActor in
                switch result {
                case .success:
                    self?.onInvalidate?()
                    self?.receiveLoop()
                case .failure:
                    break
                }
            }
        }
    }
}

extension TransactionWebSocketService: URLSessionWebSocketDelegate {
    nonisolated func urlSession(_ session: URLSession,
                     webSocketTask: URLSessionWebSocketTask,
                     didOpenWithProtocol protocol: String?) { }
    nonisolated func urlSession(_ session: URLSession,
                     webSocketTask: URLSessionWebSocketTask,
                     didCloseWith closeCode: URLSessionWebSocketTask.CloseCode,
                     reason: Data?) { }
}
