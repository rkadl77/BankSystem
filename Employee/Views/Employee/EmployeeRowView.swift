//
//  EmployeeRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct EmployeeRowView: View {
    let employee: EmployeeDTO
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(employee.isActive ? Color.bankGold.opacity(0.15) : Color.secondary.opacity(0.1))
                    .frame(width: 46, height: 46)
                Text(employee.initials).font(.subheadline.bold())
                    .foregroundColor(employee.isActive ? .bankGold : .secondary)
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(employee.fullName).font(.headline)
                Text(employee.position).font(.caption).foregroundColor(.secondary)
                Text(employee.department).font(.caption2).foregroundColor(.secondary)
            }
            Spacer()
            if !employee.isActive {
                Image(systemName: "person.slash.fill").foregroundColor(.bankDanger).font(.caption)
            }
        }
        .padding(.vertical, 4)
    }
}
