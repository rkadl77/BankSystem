//
//  CreditsView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

// MARK: — Credits List

struct CreditsView: View {
    @ObservedObject var creditVM: CreditViewModel
    @ObservedObject var accountsVM: AccountsViewModel
    let clientId: UUID
    
    @State private var showTakeCredit = false
    
    var body: some View {
        NavigationStack {
            List {
                if creditVM.credits.isEmpty {
                    Section {
                        VStack(spacing: 12) {
                            Image(systemName: "banknote")
                                .font(.system(size: 40))
                                .foregroundColor(.secondary)
                            Text("Нет активных кредитов")
                                .foregroundColor(.secondary)
                        }
                        .frame(maxWidth: .infinity)
                        .padding(.vertical, 24)
                    }
                } else {
                    ForEach(creditVM.credits) { credit in
                        NavigationLink {
                            CreditDetailView(credit: credit,
                                             creditVM: creditVM,
                                             accounts: accountsVM.accounts.filter { $0.isActive })
                        } label: {
                            CreditRowView(credit: credit, tariffName: creditVM.tariffName(for: credit.tariffId))
                        }
                    }
                }
            }
            .navigationTitle("Кредиты")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button("Взять кредит") { showTakeCredit = true }
                        .foregroundColor(.bankAccent)
                }
            }
            .sheet(isPresented: $showTakeCredit) {
                TakeCreditView(creditVM: creditVM,
                               accounts: accountsVM.accounts.filter { $0.isActive })
            }
            .overlay(alignment: .bottom) {
                if let msg = creditVM.successMessage {
                    ToastView(message: msg, isError: false).padding(.bottom, 16)
                } else if let err = creditVM.errorMessage {
                    ToastView(message: err, isError: true).padding(.bottom, 16)
                }
            }
        }
    }
}

import SwiftUI

private struct CreditRowView: View {
    let credit: Credit
    let tariffName: String
    
    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                Text(tariffName)
                    .font(.headline)
                Spacer()
                Text(credit.status.rawValue)
                    .font(.caption2.weight(.semibold))
                    .padding(.horizontal, 8).padding(.vertical, 3)
                    .background(credit.status.color.opacity(0.15))
                    .foregroundColor(credit.status.color)
                    .cornerRadius(6)
            }
            
            HStack {
                VStack(alignment: .leading, spacing: 2) {
                    Text("Осталось").font(.caption).foregroundColor(.secondary)
                    Text(credit.formattedRemaining).font(.subheadline.weight(.semibold))
                }
                Spacer()
                VStack(alignment: .trailing, spacing: 2) {
                    Text("Ставка").font(.caption).foregroundColor(.secondary)
                    Text("\(String(format: "%.1f", credit.interestRate))%").font(.subheadline.weight(.semibold))
                }
            }
            
            ProgressView(value: credit.progressFraction)
                .tint(.bankAccent)
        }
        .padding(.vertical, 4)
    }
}
