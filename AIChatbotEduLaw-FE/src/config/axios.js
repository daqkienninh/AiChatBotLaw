import axios from "axios";

// Tạo instance Axios
const api = axios.create({
  baseURL: "https://your-api-base-url.com", // Thay bằng base URL của API
  timeout: 10000, // Timeout 10 giây
  headers: {
    "Content-Type": "application/json",
    // Thêm headers nếu cần, ví dụ:
    // 'Authorization': `Bearer ${token}`
  },
});

// Interceptor cho request (tùy chọn)
api.interceptors.request.use(
  (config) => {
    // Thêm logic trước khi gửi request, ví dụ: thêm token
    // config.headers.Authorization = `Bearer ${localStorage.getItem('token')}`;
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor cho response (tùy chọn)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Xử lý lỗi toàn cục, ví dụ: log lỗi hoặc redirect khi 401
    console.error("API Error:", error.response?.status, error.message);
    return Promise.reject(error);
  }
);

export default api;
