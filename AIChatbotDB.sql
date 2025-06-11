create database AIChatbotDB
use AIChatbotDB

-- Bảng lưu thông tin người dùng đã đăng ký
CREATE TABLE Registered_user (
    user_id NVARCHAR(36) PRIMARY KEY,        -- UUID
    user_name NVARCHAR(100) NOT NULL,
    user_email VARCHAR(100) NOT NULL UNIQUE,
	Password NVARCHAR(255),
    user_status VARCHAR(50),             -- active / inactive / banned
    role VARCHAR(50),                    -- admin / user / moderator
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Bảng lưu câu hỏi của người dùng
CREATE TABLE Question (
    question_id NVARCHAR(36) PRIMARY KEY,    -- UUID
    user_id NVARCHAR(36),
    question_content NVARCHAR (4000) NOT NULL,
    ques_create_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Registered_user(user_id)
);

-- Bảng lưu câu trả lời
CREATE TABLE Answer (
    answer_id NVARCHAR(36) PRIMARY KEY,      -- UUID
    question_id NVARCHAR(36),
    ans_content NVARCHAR (4000) NOT NULL,
    ans_create_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    -- Trỏ tới văn bản luật (điều khoản) trong MongoDB (Mongo ID hoặc custom string)
    legalclause_id VARCHAR(100),        

    FOREIGN KEY (question_id) REFERENCES Question(question_id)
);

INSERT INTO Registered_user (user_id, user_name, user_email, password, user_status, role)
VALUES (
    'USR005', 
    'Alice', 
    'alice@example.com', 
    'Alice123', 
    'active', 
    'user'
);

