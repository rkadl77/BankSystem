import Foundation

final class SettingsService {
    static let shared = SettingsService()
    private let api = APIClient.shared

    func getSettings(userId: UUID) async throws -> UserSettingsDTO {
        try await api.get("\(API.settings)/settings/\(userId)")
    }

    func saveSettings(userId: UUID, theme: String?, hiddenAccountIds: [UUID]) async throws -> UserSettingsDTO {
        try await api.post("\(API.settings)/settings/\(userId)",
                           body: CreateSettingsRequest(theme: theme, hiddenAccountIds: hiddenAccountIds))
    }

    func updateTheme(userId: UUID, theme: String) async throws -> UserSettingsDTO {
        try await api.patch("\(API.settings)/settings/\(userId)/theme",
                            body: UpdateThemeRequest(theme: theme))
    }

    func updateHiddenAccounts(userId: UUID, accountIds: [UUID]) async throws -> UserSettingsDTO {
        try await api.patch("\(API.settings)/settings/\(userId)/hidden-accounts",
                            body: UpdateHiddenAccountsRequest(accountIds: accountIds))
    }
}
