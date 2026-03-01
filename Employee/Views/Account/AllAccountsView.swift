//
//  AllAccountsView.swift
//  Client
//
//  Created by Gleb Korotkov on 26.03.2026.
//


import SwiftUI

struct AllAccountsView: View {
    @ObservedObject var vm: AllAccountsViewModel
    @State private var searchText = ""
    
    var filtered: [Account] {
        if searchText.isEmpty { return vm.accounts }
        return vm.accounts.filter {
            vm.ownerName(for: $0).localizedCaseInsensitiveContains(searchText) ||
            $0.accountNumber.contains(searchText)
        }
    }
    
    var body: some View {
        NavigationStack {
            List {
                ForEach(filtered) { acc in
                    NavigationLink {
                        EmployeeAccountDetailView(account: acc)
                    } label: {
                        VStack(alignment: .leading, spacing: 4) {
                            HStack {
                                Text(vm.ownerName(for: acc))
                                    .font(.subheadline.weight(.semibold))
                                Spacer()
                                Text(acc.formattedBalance)
                                    .font(.subheadline.weight(.semibold))
                            }
                            HStack {
                                Text(acc.type.rawValue)
                                    .font(.caption).foregroundColor(.secondary)
                                Text("•")
                                    .font(.caption).foregroundColor(.secondary)
                                Text(acc.maskedNumber)
                                    .font(.caption.monospaced()).foregroundColor(.secondary)
                                Spacer()
                                Text(acc.status.rawValue)
                                    .font(.caption2.weight(.medium))
                                    .foregroundColor(acc.status.color)
                            }
                        }
                        .padding(.vertical, 2)
                    }
                }
            }
            .searchable(text: $searchText, prompt: "Поиск по клиенту или номеру")
            .navigationTitle("Все счета (\(vm.accounts.count))")
        }
    }
}
