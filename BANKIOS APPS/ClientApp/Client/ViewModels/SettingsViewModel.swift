import Foundation
import Combine
import SwiftUI

@MainActor
final class SettingsViewModel: ObservableObject {
    @Published var isDarkTheme = false
    @Published var hiddenAccountIds: Set<UUID> = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    private let userId: UUID
    init(userId: UUID) { self.userId = userId }

    func load() {
        Task {
            isLoading = true
            do {
                let s = try await SettingsService.shared.getSettings(userId: userId)
                isDarkTheme        = s.theme?.lowercased() == "dark"
                hiddenAccountIds   = Set(s.hiddenAccountIds ?? [])
            } catch NetworkError.serverError(404, _) {
                
            } catch {
                errorMessage = error.localizedDescription
            }
            isLoading = false
        }
    }

    func setTheme(_ dark: Bool) {
        isDarkTheme = dark
        Task {
            do {
                _ = try await SettingsService.shared.updateTheme(userId: userId,
                                                                  theme: dark ? "dark" : "light")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func toggleAccountVisibility(_ accountId: UUID) {
        if hiddenAccountIds.contains(accountId) {
            hiddenAccountIds.remove(accountId)
        } else {
            hiddenAccountIds.insert(accountId)
        }
        syncHiddenAccounts()
    }

    func isHidden(_ accountId: UUID) -> Bool {
        hiddenAccountIds.contains(accountId)
    }

    private func syncHiddenAccounts() {
        let ids = Array(hiddenAccountIds)
        Task {
            do {
                _ = try await SettingsService.shared.updateHiddenAccounts(userId: userId, accountIds: ids)
            } catch { errorMessage = error.localizedDescription }
        }
    }
}
