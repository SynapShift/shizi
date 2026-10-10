import XCTest
@testable import ShiziMac

final class TextCleanerTests: XCTestCase {
    func testMergesChineseVisualLines() {
        XCTAssertEqual(
            TextCleaner.clean("网页第一行\n接着的中文", preserveLineBreaks: false),
            "网页第一行接着的中文"
        )
    }

    func testAddsSpaceBetweenEnglishLines() {
        XCTAssertEqual(
            TextCleaner.clean("Hello\nworld", preserveLineBreaks: false),
            "Hello world"
        )
    }

    func testPreservesParagraphs() {
        XCTAssertEqual(
            TextCleaner.clean("第一段\n\n\n第二段", preserveLineBreaks: true),
            "第一段\n\n第二段"
        )
    }

    func testRemovesArtificialChineseSpaces() {
        XCTAssertEqual(
            TextCleaner.clean("看 见 文 字 ， 顺 手 拾 走 。", preserveLineBreaks: false),
            "看见文字，顺手拾走。"
        )
    }
}
