//
//  Color+Extensions.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//

import SwiftUI

extension Color {
    // Brand
    static let bankPrimary = Color(red: 0.07, green: 0.13, blue: 0.27)       
    static let bankAccent  = Color(red: 0.20, green: 0.60, blue: 0.90)
    static let bankGold    = Color(red: 0.95, green: 0.77, blue: 0.35)
    
    // Semantic
    static let bankSuccess = Color(red: 0.20, green: 0.78, blue: 0.55)
    static let bankDanger  = Color(red: 0.95, green: 0.35, blue: 0.35)
    static let bankWarning = Color(red: 1.00, green: 0.65, blue: 0.20)
    
    // Surfaces
    static let bankSurface = Color(UIColor.secondarySystemGroupedBackground)
    static let bankBackground = Color(UIColor.systemGroupedBackground)
}


extension Date {
    var shortFormatted: String {
        let f = DateFormatter()
        f.dateFormat = "dd.MM.yyyy"
        f.locale = Locale(identifier: "ru_RU")
        return f.string(from: self)
    }
    var longFormatted: String {
        let f = DateFormatter()
        f.dateFormat = "dd MMM yyyy, HH:mm"
        f.locale = Locale(identifier: "ru_RU")
        return f.string(from: self)
    }
}


