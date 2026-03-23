import Foundation
import Security

final class TokenStore {
    static let shared = TokenStore()
    private let account = "bankapp.jwt.token"
    private let accessGroup = "com.Gleb.BankApp"

    var token: String? {
        get { load() }
        set {
            if let v = newValue { save(v) } else { delete() }
        }
    }

    var userId: UUID? {
        get {
            guard let str = UserDefaults.standard.string(forKey: "bankapp.userId") else { return nil }
            return UUID(uuidString: str)
        }
        set { UserDefaults.standard.set(newValue?.uuidString, forKey: "bankapp.userId") }
    }

    var userRole: String? {
        get { UserDefaults.standard.string(forKey: "bankapp.userRole") }
        set { UserDefaults.standard.set(newValue, forKey: "bankapp.userRole") }
    }

    private func save(_ value: String) {
        let data = Data(value.utf8)
        let query: [CFString: Any] = [kSecClass: kSecClassGenericPassword,
                                       kSecAttrAccount: account]
        SecItemDelete(query as CFDictionary)
        var attrs = query
        attrs[kSecValueData] = data
        attrs[kSecAttrAccessible] = kSecAttrAccessibleWhenUnlocked
        SecItemAdd(attrs as CFDictionary, nil)
    }

    private func load() -> String? {
        let query: [CFString: Any] = [kSecClass: kSecClassGenericPassword,
                                       kSecAttrAccount: account,
                                       kSecReturnData: true,
                                       kSecMatchLimit: kSecMatchLimitOne]
        var item: CFTypeRef?
        guard SecItemCopyMatching(query as CFDictionary, &item) == errSecSuccess,
              let data = item as? Data else { return nil }
        return String(data: data, encoding: .utf8)
    }

    private func delete() {
        let query: [CFString: Any] = [kSecClass: kSecClassGenericPassword,
                                       kSecAttrAccount: account]
        SecItemDelete(query as CFDictionary)
        userId = nil
        userRole = nil
    }
}
