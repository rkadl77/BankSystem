//
//  TariffsViewModel.swift
//  Employee
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Combine

@MainActor
final class TariffsViewModel: ObservableObject {
    @Published var tariffs: [CreditTariffDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?

    @Published var newName = ""
    @Published var newRate = ""
    @Published var newDescription = ""

    func load() {
        Task {
            isLoading = true
            do { tariffs = try await CreditService.shared.getAllTariffs() }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func create() {
        guard !newName.trimmingCharacters(in: .whitespaces).isEmpty else {
            errorMessage = "Введите название"; return
        }
        guard let rate = Double(newRate), rate > 0, rate < 200 else {
            errorMessage = "Введите корректную ставку (0–200%)"; return
        }
        Task {
            do {
                let t = try await CreditService.shared.createTariff(
                    name: newName.trimmingCharacters(in: .whitespaces),
                    interestRate: rate,
                    description: newDescription)
                tariffs.append(t)
                newName = ""; newRate = ""; newDescription = ""
                flash("Тариф «\(t.name)» создан ✓")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func toggleActive(_ tariff: CreditTariffDTO) {
        Task {
            do {
                let updated = try await CreditService.shared.updateTariff(
                    id: tariff.id, name: tariff.name,
                    interestRate: tariff.interestRate,
                    description: tariff.description,
                    isActive: !tariff.isActive)
                if let i = tariffs.firstIndex(where: { $0.id == tariff.id }) { tariffs[i] = updated }
            } catch { errorMessage = error.localizedDescription }
        }
    }

    private func flash(_ msg: String) {
        successMessage = msg
        Task { try? await Task.sleep(nanoseconds: 2_500_000_000); successMessage = nil }
    }
}
