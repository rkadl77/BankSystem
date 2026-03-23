import SwiftUI

struct CreditRatingView: View {
    @StateObject private var vm: CreditRatingViewModel

    init(clientId: UUID) {
        _vm = StateObject(wrappedValue: CreditRatingViewModel(clientId: clientId))
    }

    var body: some View {
        List {
            if vm.isLoading {
                Section { ProgressView("Загрузка...").frame(maxWidth: .infinity) }
            }

            if let rating = vm.rating {
                Section("Кредитный рейтинг клиента") {
                    VStack(spacing: 12) {
                        ZStack {
                            Circle()
                                .stroke(Color.secondary.opacity(0.2), lineWidth: 12)
                                .frame(width: 120, height: 120)
                            Circle()
                                .trim(from: 0, to: CGFloat(max(0, rating.rating - 300)) / 550)
                                .stroke(rating.ratingColor, style: StrokeStyle(lineWidth: 12, lineCap: .round))
                                .frame(width: 120, height: 120)
                                .rotationEffect(.degrees(-90))
                            VStack(spacing: 2) {
                                Text("\(rating.rating)")
                                    .font(.system(size: 32, weight: .bold, design: .rounded))
                                    .foregroundColor(rating.ratingColor)
                                Text(rating.ratingLabel).font(.caption).foregroundColor(.secondary)
                            }
                        }
                        .frame(maxWidth: .infinity)
                        .padding(.vertical, 4)

                        if let desc = rating.description {
                            Text(desc).font(.subheadline).foregroundColor(.secondary)
                                .multilineTextAlignment(.center)
                        }
                    }

                    LabeledContent("Вовремя", value: "\(Int(rating.onTimePercentage))%")
                    LabeledContent("Просрочено", value: "\(rating.overdueCount) из \(rating.totalCount)")
                        .foregroundColor(rating.overdueCount > 0 ? .bankDanger : .primary)
                }
            }

            if !vm.overdueCredits.isEmpty {
                Section("Просроченные кредиты (\(vm.overdueCredits.count))") {
                    ForEach(vm.overdueCredits) { credit in
                        VStack(alignment: .leading, spacing: 4) {
                            HStack {
                                Text(credit.tariffName).font(.subheadline.weight(.medium))
                                Spacer()
                                Text(credit.statusLabel).font(.caption.weight(.bold))
                                    .foregroundColor(credit.statusColor)
                            }
                            Text("\(credit.formattedRemaining) остаток · \(credit.formattedRate)")
                                .font(.caption).foregroundColor(.secondary)
                        }
                        .padding(.vertical, 2)
                    }
                }
            } else if !vm.isLoading {
                Section { Text("Нет просроченных платежей ✓").foregroundColor(.bankSuccess) }
            }

            if let err = vm.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }
        }
        .navigationTitle("Кредитный рейтинг")
        .onAppear { vm.load() }
        .refreshable { vm.load() }
    }
}
