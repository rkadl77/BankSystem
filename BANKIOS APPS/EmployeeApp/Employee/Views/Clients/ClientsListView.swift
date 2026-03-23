//
//  ClientsListView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct ClientsListView: View {
    @ObservedObject var vm: ClientsViewModel
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

                Section("Клиенты (\(vm.filtered.count))") {
                    if vm.filtered.isEmpty && !vm.isLoading {
                        Text("Нет клиентов").foregroundColor(.secondary)
                    }
                    ForEach(vm.filtered) { client in
                        NavigationLink(destination: ClientDetailView(client: client, clientsVM: vm)) {
                            ClientRowView(client: client)
                        }
                    }
                }
            }
            .navigationTitle("Клиенты")
            .searchable(text: $vm.searchText, prompt: "Имя, email или телефон")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showCreate = true } label: { Image(systemName: "person.badge.plus") }
                }
            }
            .sheet(isPresented: $showCreate) { CreateClientSheet(vm: vm) }
            .refreshable { vm.load() }
        }
    }
}
