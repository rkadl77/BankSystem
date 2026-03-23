import Foundation
import Combine

@MainActor
final class CreditRatingViewModel: ObservableObject {
    @Published var rating: CreditRatingDTO?
    @Published var overdueCredits: [CreditDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    private let clientId: UUID
    init(clientId: UUID) { self.clientId = clientId }

    func load() {
        Task {
            isLoading = true; errorMessage = nil
            async let r  = CreditService.shared.getCreditRating(clientId: clientId)
            async let oc = CreditService.shared.getOverdueCredits(clientId: clientId)
            do {
                rating         = try await r
                overdueCredits = try await oc
            } catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }
}
