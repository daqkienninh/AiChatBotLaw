
-- Bảng lưu câu hỏi của người dùng
CREATE TABLE Question (
    question_id NVARCHAR(36) PRIMARY KEY,    -- UUID
    user_id NVARCHAR(36),
    question_content NVARCHAR (4000) NOT NULL,
    ques_create_at DATETIME DEFAULT CURRENT_TIMESTAMP,
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

CREATE TABLE ChatRoom (
    ChatId NVARCHAR(36) PRIMARY KEY,
    UserId NVARCHAR(36) NOT NULL,
    CreatedAt DATETIME,
);
CREATE TABLE ChatRoomQuestion (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ChatId NVARCHAR(36) NOT NULL,
    QuestionId NVARCHAR(36) NOT NULL,
    FOREIGN KEY (ChatId) REFERENCES ChatRoom(ChatId) ON DELETE NO ACTION,
    FOREIGN KEY (QuestionId) REFERENCES Question(question_id) ON DELETE NO ACTION
);


