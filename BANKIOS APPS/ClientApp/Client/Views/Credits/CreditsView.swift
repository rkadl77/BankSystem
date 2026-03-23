//
//  CreditsView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

struct CreditsView: View {
    @ObservedObject var creditVM: CreditViewModel
    @ObservedObject var accountsVM: AccountsViewModel
    @State private var showTake = false

    var body: some View {
        NavigationStack {
            List {
                if let err = creditVM.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
                }
                if let ok = creditVM.successMessage {
                    Section { Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess) }
                }
                if creditVM.isLoading {
                    Section { ProgressView("Загрузка...").frame(maxWidth: .infinity) }
                } else if creditVM.credits.isEmpty {
                    Section {
                        VStack(spacing: 12) {
                            Image(systemName: "banknote").font(.system(size: 44)).foregroundColor(.secondary)
                            Text("Нет кредитов").foregroundColor(.secondary)
                        }.frame(maxWidth: .infinity).padding(.vertical, 24)
                    }
                } else {
                    Section("Мои кредиты") {
                        ForEach(creditVM.credits) { credit in
                            NavigationLink(destination: CreditDetailView(credit: credit, creditVM: creditVM,
                                                                          activeAccounts: accountsVM.accounts.filter { $0.isActive })) {
                                CreditRowView(credit: credit)
                            }
                        }
                    }
                }
            }
            .navigationTitle("Кредиты")
            .toolbar { ToolbarItem(placement: .navigationBarTrailing) { Button("Взять") { showTake = true }.foregroundColor(.bankAccent) } }
            .sheet(isPresented: $showTake) {
                TakeCreditView(creditVM: creditVM, activeAccounts: accountsVM.accounts.filter { $0.isActive })
            }
            .refreshable { creditVM.load() }
        }
    }
}

struct CreditRowView: View {
    let credit: CreditDTO
    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                Text(credit.tariffName).font(.headline)
                Spacer()
                Text(credit.statusLabel)
                    .font(.caption2.weight(.semibold))
                    .padding(.horizontal, 8).padding(.vertical, 3)
                    .background(credit.statusColor.opacity(0.15))
                    .foregroundColor(credit.statusColor).cornerRadius(6)
            }
            HStack {
                VStack(alignment: .leading, spacing: 1) {
                    Text("Остаток").font(.caption).foregroundColor(.secondary)
                    Text(credit.formattedRemaining).font(.subheadline.weight(.semibold))
                }
                Spacer()
                VStack(alignment: .trailing, spacing: 1) {
                    Text("Ставка").font(.caption).foregroundColor(.secondary)
                    Text(credit.formattedRate).font(.subheadline.weight(.semibold))
                }
            }
            ProgressView(value: credit.progressFraction).tint(.bankAccent)
        }
        .padding(.vertical, 4)
    }
}
