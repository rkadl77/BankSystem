//
//  TariffViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//


import Foundation
import Combine

// MARK: — Tariffs

final class TariffViewModel: ObservableObject {
    @Published var tariffs: [CreditTariff] = []
    @Published var newName = ""
    @Published var newRate = ""
    @Published var errorMessage: String?
    @Published var successMessage: String?
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    private let employeeId: UUID
    
    init(employeeId: UUID) {
        self.employeeId = employeeId
        db.$tariffs
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
        load()
    }
    
    func load() { tariffs = db.tariffs }
    
    func createTariff() {
        guard !newName.isEmpty else { errorMessage = "Введите название"; return }
        guard let rate = Double(newRate), rate > 0 else { errorMessage = "Неверная ставка"; return }
        db.createTariff(name: newName, rate: rate, employeeId: employeeId)
        newName = ""; newRate = ""
        successMessage = "Тариф создан ✓"
        DispatchQueue.main.asyncAfter(deadline: .now() + 2) { [weak self] in
            self?.successMessage = nil
        }
    }
}
