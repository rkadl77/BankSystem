//
//  CreditDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//

import SwiftUI

struct CreditDetailView: View {
    let credit: CreditDTO
    @ObservedObject var creditVM: CreditViewModel
    let activeAccounts: [AccountDTO]
    @State private var selectedAccountId: UUID?
    @State private var payText = ""

    var body: some View {
        List {
            Section("Информация") {
                LabeledContent("Тариф",   value: credit.tariffName)
                LabeledContent("Сумма",   value: credit.formattedAmount)
                LabeledContent("Остаток", value: credit.formattedRemaining)
                LabeledContent("Ставка",  value: credit.formattedRate)
                LabeledContent("Срок",    value: "\(credit.termMonths) мес.")
                LabeledContent("Выдан",   value: credit.startDate.shortFormatted)
                HStack {
                    Text("Дата платежа"); Spacer()
                    Text(credit.paymentDueDate.shortFormatted)
                        .foregroundColor(credit.isOverdue ? .red : .primary)
                        .fontWeight(credit.isOverdue ? .semibold : .regular)
                }
                if credit.isOverdue {
                    HStack {
                        Image(systemName: "exclamationmark.circle.fill").foregroundColor(.red)
                        Text("Просрочено на \(credit.daysOverdue) дн.")
                            .foregroundColor(.red).fontWeight(.semibold)
                    }
                }
                if let e = credit.endDate { LabeledContent("Закрыт", value: e.shortFormatted) }
                HStack {
                    Text("Статус"); Spacer()
                    Text(credit.statusLabel).foregroundColor(credit.statusColor).fontWeight(.semibold)
                }
            }
            Section {
                ProgressView(value: credit.progressFraction).tint(.bankAccent)
                Text("\(Int(credit.progressFraction * 100))% погашено")
                    .font(.caption).foregroundColor(.secondary)
            }

            if credit.status.lowercased() == "active" {
                Section("Внести платёж") {
                    if activeAccounts.isEmpty {
                        Text("Нет активных счетов").foregroundColor(.secondary)
                    } else {
                        Picker("Счёт", selection: $selectedAccountId) {
                            Text("Счёт...").tag(UUID?.none)
                            ForEach(activeAccounts) { a in
                                Text("\(a.currency) — \(a.formattedBalance)").tag(a.id as UUID?)
                            }
                        }
                        HStack {
                            TextField("Сумма платежа", text: $payText).keyboardType(.decimalPad)
                            Button("Оплатить") {
                                guard selectedAccountId != nil, let amt = Double(payText), amt > 0 else { return }
                                creditVM.repay(credit, amount: amt); payText = ""
                            }
                            .foregroundColor(.bankAccent).fontWeight(.semibold)
                            .disabled(selectedAccountId == nil || Double(payText) == nil)
                        }
                    }
                }
            }
            if let err = creditVM.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }
            if let ok = creditVM.successMessage {
                Section { Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess) }
            }
        }
        .navigationTitle("Кредит")
        .onAppear { selectedAccountId = activeAccounts.first?.id }
    }
}
