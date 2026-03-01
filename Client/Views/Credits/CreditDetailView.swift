//
//  CreditDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//

import SwiftUI

struct CreditDetailView: View {
    let credit: Credit
    @ObservedObject var creditVM: CreditViewModel
    let accounts: [Account]
    
    @State private var selectedAccountId: UUID?
    @State private var paymentAmount: String = ""
    
    var body: some View {
        List {
            Section("Информация") {
                LabeledContent("Сумма кредита", value: credit.formattedAmount)
                LabeledContent("Остаток", value: credit.formattedRemaining)
                LabeledContent("Ставка", value: "\(String(format: "%.1f", credit.interestRate))%")
                LabeledContent("Дата выдачи", value: credit.startDate.shortFormatted)
                LabeledContent("Дата закрытия", value: credit.endDate.shortFormatted)
                LabeledContent("Ежедневный платёж", value: String(format: "%.2f ₽", credit.dailyPayment))
            }
            
            if credit.status == .active {
                Section("Погашение") {
                    Picker("Счёт списания", selection: $selectedAccountId) {
                        Text("Выберите счёт").tag(UUID?.none)
                        ForEach(accounts) { acc in
                            Text("\(acc.type.rawValue) — \(acc.formattedBalance)").tag(acc.id as UUID?)
                        }
                    }
                    HStack {
                        TextField("Сумма погашения", text: $paymentAmount)
                            .keyboardType(.decimalPad)
                        Button("Оплатить") {
                            guard let accId = selectedAccountId,
                                  let amount = Double(paymentAmount) else { return }
                            creditVM.repayCredit(credit, from: accId, amount: amount)
                            paymentAmount = ""
                        }
                        .foregroundColor(.bankAccent)
                        .fontWeight(.semibold)
                    }
                }
            }
        }
        .navigationTitle("Кредит")
        .onAppear { selectedAccountId = accounts.first?.id }
    }
}
