//
//  TariffsView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct TariffsView: View {
    @ObservedObject var vm: TariffsViewModel
    @State private var showCreate = false

    var body: some View {
        NavigationStack {
            List {
                if let err = vm.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.triangle.fill").foregroundColor(.bankDanger) }
                }
                if let ok = vm.successMessage {
                    Section { Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess) }
                }
                if vm.isLoading {
                    Section { ProgressView("Загрузка...").frame(maxWidth: .infinity) }
                }

                let active   = vm.tariffs.filter { $0.isActive }
                let inactive = vm.tariffs.filter { !$0.isActive }

                if !active.isEmpty {
                    Section("Активные (\(active.count))") {
                        ForEach(active) { t in TariffRowView(tariff: t, vm: vm) }
                    }
                }
                if !inactive.isEmpty {
                    Section("Неактивные (\(inactive.count))") {
                        ForEach(inactive) { t in TariffRowView(tariff: t, vm: vm) }
                    }
                }
                if !vm.isLoading && vm.tariffs.isEmpty {
                    Section { Text("Нет тарифов. Создайте первый!").foregroundColor(.secondary) }
                }
            }
            .navigationTitle("Кредитные тарифы")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showCreate = true } label: { Image(systemName: "plus") }
                }
            }
            .sheet(isPresented: $showCreate) { CreateTariffSheet(vm: vm) }
            .refreshable { vm.load() }
        }
    }
}
