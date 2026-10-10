// swift-tools-version: 5.9

import PackageDescription

let package = Package(
    name: "ShiziMac",
    platforms: [.macOS(.v13)],
    products: [
        .executable(name: "ShiziMac", targets: ["ShiziMac"])
    ],
    targets: [
        .executableTarget(
            name: "ShiziMac",
            linkerSettings: [
                .linkedFramework("AppKit"),
                .linkedFramework("Carbon"),
                .linkedFramework("CoreGraphics"),
                .linkedFramework("ServiceManagement"),
                .linkedFramework("Vision")
            ]
        ),
        .testTarget(name: "ShiziMacTests", dependencies: ["ShiziMac"])
    ]
)
