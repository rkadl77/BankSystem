//
//  ClientsListView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

// MARK: — Clients List

struct ClientsListView: View {
    @ObservedObject var vm: AllClientsViewModel
    @State private var showAddClient = false
    
    var body: some View {
        NavigationStack {
            List {
                ForEach(vm.filtered) { client in
                    NavigationLink {
                        ClientDetailView(client: client, clientsVM: vm)
                    } label: {
                        ClientRowView(client: client)
                    }
                }
            }
            .searchable(text: $vm.searchText, prompt: "Поиск по имени или email")
            .navigationTitle("Клиенты (\(vm.clients.count))")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showAddClient = true } label: {
                        Image(systemName: "person.badge.plus")
                    }
                }
            }
            .sheet(isPresented: $showAddClient) {
                AddUserSheet(role: .client) { name, email, phone in
                    vm.createClient(fullName: name, email: email, phone: phone)
                    showAddClient = false
                }
            }
        }
    }
}

private struct ClientRowView: View {
    let client: User
    
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(client.isBlocked ? Color.bankDanger.opacity(0.15) : Color.bankAccent.opacity(0.15))
                    .frame(width: 44, height: 44)
                Text(client.initials)
                    .font(.subheadline.bold())
                    .foregroundColor(client.isBlocked ? .bankDanger : .bankAccent)
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(client.fullName).font(.headline)
                Text(client.email).font(.caption).foregroundColor(.secondary)
            }
            Spacer()
            if client.isBlocked {
                Image(systemName: "lock.fill")
                    .foregroundColor(.bankDanger)
                    .font(.caption)
            }
        }
        .padding(.vertical, 2)
    }
}
