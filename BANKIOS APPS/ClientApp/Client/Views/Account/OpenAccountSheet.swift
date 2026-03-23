




import SwiftUI

struct OpenAccountSheet: View {
    @Binding var currency: String
    let onConfirm: () -> Void
    @Environment(\.dismiss) var dismiss
    let currencies = [("RUB", "Рублёвый ₽"), ("USD", "Долларовый $"), ("EUR", "Евро €")]

    var body: some View {
        NavigationStack {
            Form {
                Section("Валюта") {
                    Picker("Валюта", selection: $currency) {
                        ForEach(currencies, id: \.0) { c in Text(c.1).tag(c.0) }
                    }.pickerStyle(.inline)
                }
                Section { Button("Открыть счёт", action: onConfirm).foregroundColor(.bankAccent).fontWeight(.semibold) }
            }
            .navigationTitle("Новый счёт")
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Отмена") { dismiss() } } }
        }
        .presentationDetents([.medium])
    }
}
