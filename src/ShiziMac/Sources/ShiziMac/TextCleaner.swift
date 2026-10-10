import Foundation

enum TextCleaner {
    static func clean(_ input: String, preserveLineBreaks: Bool) -> String {
        let normalized = input
            .replacingOccurrences(of: "\r\n", with: "\n")
            .replacingOccurrences(of: "\r", with: "\n")
        let lines = normalized.split(separator: "\n", omittingEmptySubsequences: false)
            .map { collapseWhitespace(String($0)).trimmingCharacters(in: .whitespaces) }

        if preserveLineBreaks {
            return lines.joined(separator: "\n")
                .replacingOccurrences(of: "\n{3,}", with: "\n\n", options: .regularExpression)
                .trimmingCharacters(in: .whitespacesAndNewlines)
        }

        var output = ""
        var previousWasBlank = false
        for line in lines {
            if line.isEmpty {
                if !output.isEmpty { previousWasBlank = true }
                continue
            }

            if output.isEmpty {
                output = line
            } else if previousWasBlank || shouldKeepBreak(after: output, before: line) {
                output += "\n\n" + line
            } else if needsSpace(between: output.last, and: line.first) {
                output += " " + line
            } else {
                output += line
            }
            previousWasBlank = false
        }

        return output.trimmingCharacters(in: .whitespacesAndNewlines)
    }

    private static func collapseWhitespace(_ value: String) -> String {
        var result = value.replacingOccurrences(of: "[\\t ]+", with: " ", options: .regularExpression)
        result = result.replacingOccurrences(
            of: "(?<=[\\p{script=Han}]) (?=[\\p{script=Han}，。！？；：、])",
            with: "",
            options: .regularExpression
        )
        result = result.replacingOccurrences(
            of: "(?<=[，。！？；：、]) (?=[\\p{script=Han}])",
            with: "",
            options: .regularExpression
        )
        return result
    }

    private static func shouldKeepBreak(after previous: String, before current: String) -> Bool {
        guard let first = current.first else { return false }
        if "•●▪■-—·".contains(first) { return true }
        return previous.hasSuffix("。") || previous.hasSuffix("！") || previous.hasSuffix("？")
    }

    private static func needsSpace(between left: Character?, and right: Character?) -> Bool {
        guard let left, let right else { return false }
        return left.isASCII && right.isASCII && (left.isLetter || left.isNumber) && (right.isLetter || right.isNumber)
    }
}
