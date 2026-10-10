import CoreGraphics
import Vision

enum VisionOCR {
    static func recognize(_ image: CGImage) throws -> String {
        let request = VNRecognizeTextRequest()
        request.recognitionLevel = .accurate
        request.usesLanguageCorrection = true
        request.recognitionLanguages = ["zh-Hans", "zh-Hant", "en-US"]

        let handler = VNImageRequestHandler(cgImage: image, orientation: .up)
        try handler.perform([request])

        let observations = (request.results ?? []).sorted {
            if abs($0.boundingBox.maxY - $1.boundingBox.maxY) > 0.015 {
                return $0.boundingBox.maxY > $1.boundingBox.maxY
            }
            return $0.boundingBox.minX < $1.boundingBox.minX
        }

        return observations
            .compactMap { $0.topCandidates(1).first?.string }
            .joined(separator: "\n")
    }
}
