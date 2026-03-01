//
//  CreditViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

final class CreditViewModel: ObservableObject {
    @Published var credits: [Credit] = []
    @Published var tariffs: [CreditTariff] = []
    @Published var errorMessage: String?
    @Published var successMessage: String?
    
    // Take credit form
    @Published var selectedTariff: CreditTariff?
    @Published var selectedAccountId: UUID?
    @Published var creditAmount: String = ""
    @Published var termDays: String = "30"
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    private let clientId: UUID
    
    init(clientId: UUID) {
        self.clientId = clientId
        observeDB()
        load()
    }
    
    private func observeDB() {
        db.$credits
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
    }
    
    func load() {
        credits = db.credits(for: clientId)
        tariffs = db.activeTariffs()
    }
    
    func takeCredit() -> Bool {
        guard let tariff = selectedTariff,
              let accountId = selectedAccountId,
              let amount = Double(creditAmount),
              let term = Int(termDays) else {
            errorMessage = "Заполните все поля"
            return false
        }
        
        let ok = db.takeCredit(clientId: clientId, accountId: accountId,
                               tariffId: tariff.id, amount: amount, termDays: term)
        if ok {
            successMessage = "Кредит оформлен!"
            resetForm()
        } else {
            errorMessage = "Не удалось оформить кредит. Проверьте условия тарифа."
        }
        return ok
    }
    
    func repayCredit(_ credit: Credit, from accountId: UUID, amount: Double) {
        let ok = db.repayCredit(creditId: credit.id, accountId: accountId, amount: amount)
        if ok {
            successMessage = "Платёж внесён"
        } else {
            errorMessage = "Недостаточно средств или ошибка"
        }
    }
    
    private func resetForm() {
        selectedTariff = nil
        selectedAccountId = nil
        creditAmount = ""
        termDays = "30"
    }
    
    func tariffName(for id: UUID) -> String {
        tariffs.first { $0.id == id }?.name ?? "Тариф удалён"
    }
}
