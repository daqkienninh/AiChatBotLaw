import sys
import os
import pdfplumber

def extract_text(pdf_path):
    full_text = ""
    with pdfplumber.open(pdf_path) as pdf:
        for page in pdf.pages:
            text = page.extract_text()
            if text:
                full_text += text + "\n"
    return full_text

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Missing PDF path", file=sys.stderr)
        sys.exit(1)

    pdf_path = sys.argv[1]

    try:
        text = extract_text(pdf_path)

        # Tạo thư mục outputs nếu chưa có
        output_folder = os.path.join(os.getcwd(), "outputs")
        os.makedirs(output_folder, exist_ok=True)

        # Lưu file txt
        txt_file_name = os.path.splitext(os.path.basename(pdf_path))[0] + ".txt"
        txt_file_path = os.path.join(output_folder, txt_file_name)

        with open(txt_file_path, "w", encoding="utf-8") as f:
            f.write(text)

        # In ra đường dẫn để C# đọc được
        print(txt_file_path)

    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)
