# API Documentation - BeLecas E-commerce System

## Tổng quan
Hệ thống API cho ứng dụng e-commerce BeLecas được xây dựng bằng ASP.NET Core với MongoDB làm cơ sở dữ liệu. API sử dụng JWT authentication và hỗ trợ role-based authorization.

## Base URL

### Development
```
http://localhost:5000/api
```

### Production
```
https://lecas-be.onrender.com/api
```

## Authentication
Hầu hết các API yêu cầu authentication thông qua JWT token. Token được gửi trong header:
```
Authorization: Bearer <jwt_token>
```

## Response Format
Tất cả API responses đều theo format:
```json
{
  "success": true/false,
  "message": "Success/Error message",
  "data": {...},
  "errors": [...]
}
```

---

## 1. Authentication APIs

### 1.1 Đăng nhập
- **Path:** `POST /api/auth/login`
- **Mô tả:** Đăng nhập bằng email và password
- **Input:**
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Output:**
  ```json
  {
    "success": true,
    "message": "Login successful",
    "data": {
      "accessToken": "jwt_token_here",
      "refreshToken": "refresh_token_here",
      "expiresAt": "2024-01-01T12:00:00Z",
      "user": {
        "id": "user_id",
        "email": "user@example.com",
        "name": "User Name",
        "role": "user"
      },
      "isNewUser": false
    }
  }
  ```

### 1.2 Đăng nhập Google
- **Path:** `POST /api/auth/google-login`
- **Mô tả:** Đăng nhập bằng Google OAuth
- **Input:**
  ```json
  {
    "accessToken": "google_access_token"
  }
  ```
- **Output:** Tương tự như login thường

### 1.3 Refresh Token
- **Path:** `POST /api/auth/refresh-token`
- **Mô tả:** Làm mới access token
- **Input:**
  ```json
  {
    "refreshToken": "refresh_token_here"
  }
  ```
- **Output:** Tương tự như login

### 1.4 Đăng xuất
- **Path:** `POST /api/auth/logout`
- **Mô tả:** Đăng xuất và vô hiệu hóa token
- **Authorization:** Required
- **Input:**
  ```json
  {
    "refreshToken": "refresh_token_here"
  }
  ```
- **Output:**
  ```json
  {
    "success": true,
    "message": "Logout successful"
  }
  ```

### 1.5 Xác thực Email
- **Path:** `POST /api/auth/verify-email`
- **Mô tả:** Xác thực email đăng ký
- **Input:**
  ```json
  {
    "token": "verification_token"
  }
  ```

### 1.6 Gửi lại Email Xác thực
- **Path:** `POST /api/auth/resend-verification`
- **Mô tả:** Gửi lại email xác thực
- **Input:**
  ```json
  {
    "email": "user@example.com"
  }
  ```

### 1.7 Lấy thông tin User hiện tại
- **Path:** `GET /api/auth/me`
- **Mô tả:** Lấy thông tin user đang đăng nhập
- **Authorization:** Required
- **Output:**
  ```json
  {
    "success": true,
    "data": {
      "id": "user_id",
      "email": "user@example.com",
      "name": "User Name",
      "role": "user"
    }
  }
  ```

---

## 2. User Management APIs

### 2.1 Lấy Profile
- **Path:** `GET /api/users/profile`
- **Mô tả:** Lấy thông tin profile của user
- **Authorization:** Required
- **Output:** UserDto object

### 2.2 Cập nhật Profile
- **Path:** `PUT /api/users/profile`
- **Mô tả:** Cập nhật thông tin profile
- **Authorization:** Required
- **Input:**
  ```json
  {
    "name": "New Name",
    "phone": "0123456789",
    "dateOfBirth": "1990-01-01"
  }
  ```

### 2.3 Đổi mật khẩu
- **Path:** `POST /api/users/change-password`
- **Mô tả:** Thay đổi mật khẩu
- **Authorization:** Required
- **Input:**
  ```json
  {
    "currentPassword": "old_password",
    "newPassword": "new_password"
  }
  ```

### 2.4 Thêm địa chỉ
- **Path:** `POST /api/users/addresses`
- **Mô tả:** Thêm địa chỉ mới cho user
- **Authorization:** Required
- **Input:**
  ```json
  {
    "type": "home",
    "address": "123 Street",
    "city": "Ho Chi Minh",
    "district": "District 1",
    "ward": "Ward 1",
    "isDefault": true
  }
  ```

### 2.5 Xóa địa chỉ
- **Path:** `DELETE /api/users/addresses/{addressId}`
- **Mô tả:** Xóa địa chỉ
- **Authorization:** Required

---

## 3. Product APIs

### 3.1 Lấy danh sách sản phẩm
- **Path:** `GET /api/products`
- **Mô tả:** Lấy danh sách sản phẩm với filter
- **Query Parameters:**
  - `categoryId`: ID danh mục
  - `minPrice`: Giá tối thiểu
  - `maxPrice`: Giá tối đa
  - `sortBy`: Sắp xếp (price, name, date)
  - `sortOrder`: Thứ tự (asc, desc)
  - `page`: Trang hiện tại
  - `pageSize`: Số item mỗi trang
- **Output:** List<ProductDto>

### 3.2 Lấy chi tiết sản phẩm
- **Path:** `GET /api/products/{id}`
- **Mô tả:** Lấy thông tin chi tiết sản phẩm
- **Output:** ProductDto

### 3.3 Lấy danh mục sản phẩm
- **Path:** `GET /api/products/categories`
- **Mô tả:** Lấy tất cả danh mục
- **Output:** List<CategoryDto>

### 3.4 Lấy đánh giá sản phẩm
- **Path:** `GET /api/products/{id}/reviews`
- **Mô tả:** Lấy đánh giá của sản phẩm
- **Output:** List<ReviewDto>

### 3.5 Lấy sản phẩm liên quan
- **Path:** `GET /api/products/{id}/related`
- **Mô tả:** Lấy sản phẩm liên quan
- **Output:** List<ProductDto>

### 3.6 Tạo sản phẩm mới (Admin)
- **Path:** `POST /api/products`
- **Mô tả:** Tạo sản phẩm mới
- **Authorization:** Admin required
- **Input:**
  ```json
  {
    "name": "Product Name",
    "description": "Product description",
    "price": 100000,
    "categoryId": "category_id",
    "images": ["url1", "url2"],
    "specifications": {...}
  }
  ```

### 3.7 Cập nhật sản phẩm (Admin)
- **Path:** `PUT /api/products/{id}`
- **Mô tả:** Cập nhật thông tin sản phẩm
- **Authorization:** Admin required

### 3.8 Xóa sản phẩm (Admin)
- **Path:** `DELETE /api/products/{id}`
- **Mô tả:** Xóa sản phẩm
- **Authorization:** Admin required

### 3.9 Sản phẩm bán chạy
- **Path:** `GET /api/products/top-selling`
- **Mô tả:** Lấy sản phẩm bán chạy nhất
- **Output:** List<ProductDto>

### 3.10 Sản phẩm flash sale
- **Path:** `GET /api/products/flash-sale`
- **Mô tả:** Lấy sản phẩm flash sale
- **Output:** List<ProductDto>

---

## 4. Category APIs

### 4.1 Lấy tất cả danh mục
- **Path:** `GET /api/categories`
- **Mô tả:** Lấy danh sách tất cả danh mục
- **Output:** List<CategoryDto>

### 4.2 Lấy chi tiết danh mục
- **Path:** `GET /api/categories/{id}`
- **Mô tả:** Lấy thông tin chi tiết danh mục
- **Output:** CategoryDto

### 4.3 Lấy sản phẩm theo danh mục
- **Path:** `GET /api/categories/{id}/products`
- **Mô tả:** Lấy sản phẩm thuộc danh mục
- **Output:** List<ProductDto>

### 4.4 Tạo danh mục mới (Admin)
- **Path:** `POST /api/categories`
- **Mô tả:** Tạo danh mục mới
- **Authorization:** Admin required
- **Input:**
  ```json
  {
    "name": "Category Name",
    "description": "Category description",
    "imageUrl": "category_image_url"
  }
  ```

### 4.5 Cập nhật danh mục (Admin)
- **Path:** `PUT /api/categories/{id}`
- **Mô tả:** Cập nhật thông tin danh mục
- **Authorization:** Admin required

### 4.6 Xóa danh mục (Admin)
- **Path:** `DELETE /api/categories/{id}`
- **Mô tả:** Xóa danh mục
- **Authorization:** Admin required

---

## 5. Cart APIs

### 5.1 Lấy giỏ hàng
- **Path:** `GET /api/cart`
- **Mô tả:** Lấy thông tin giỏ hàng của user
- **Authorization:** Required
- **Output:** CartDto

### 5.2 Thêm vào giỏ hàng
- **Path:** `POST /api/cart/items`
- **Mô tả:** Thêm sản phẩm vào giỏ hàng
- **Authorization:** Required
- **Input:**
  ```json
  {
    "productId": "product_id",
    "quantity": 2
  }
  ```

### 5.3 Cập nhật số lượng
- **Path:** `PUT /api/cart/items/{itemId}`
- **Mô tả:** Cập nhật số lượng sản phẩm trong giỏ hàng
- **Authorization:** Required
- **Input:**
  ```json
  {
    "quantity": 3
  }
  ```

### 5.4 Xóa khỏi giỏ hàng
- **Path:** `DELETE /api/cart/items/{itemId}`
- **Mô tả:** Xóa sản phẩm khỏi giỏ hàng
- **Authorization:** Required

### 5.5 Xóa toàn bộ giỏ hàng
- **Path:** `DELETE /api/cart`
- **Mô tả:** Xóa toàn bộ giỏ hàng
- **Authorization:** Required

### 5.6 Lấy tổng quan giỏ hàng
- **Path:** `GET /api/cart/summary`
- **Mô tả:** Lấy thông tin tổng quan giỏ hàng
- **Authorization:** Required
- **Output:** CartSummaryDto

---

## 6. Order APIs

### 6.1 Tạo đơn hàng
- **Path:** `POST /api/orders`
- **Mô tả:** Tạo đơn hàng mới từ giỏ hàng
- **Authorization:** Required
- **Input:**
  ```json
  {
    "shippingAddress": {
      "type": "home",
      "address": "123 Street",
      "city": "Ho Chi Minh",
      "district": "District 1",
      "ward": "Ward 1"
    },
    "paymentMethod": "cod",
    "note": "Delivery note"
  }
  ```

### 6.2 Lấy danh sách đơn hàng
- **Path:** `GET /api/orders`
- **Mô tả:** Lấy danh sách đơn hàng của user
- **Authorization:** Required
- **Query Parameters:**
  - `status`: Trạng thái đơn hàng
  - `fromDate`: Từ ngày
  - `toDate`: Đến ngày
- **Output:** List<OrderDto>

### 6.3 Lấy chi tiết đơn hàng
- **Path:** `GET /api/orders/{id}`
- **Mô tả:** Lấy thông tin chi tiết đơn hàng
- **Authorization:** Required
- **Output:** OrderDto

### 6.4 Hủy đơn hàng
- **Path:** `PUT /api/orders/{id}/cancel`
- **Mô tả:** Hủy đơn hàng
- **Authorization:** Required
- **Input:**
  ```json
  {
    "reason": "Reason for cancellation"
  }
  ```

### 6.5 Tạo đánh giá đơn hàng
- **Path:** `POST /api/orders/{id}/reviews`
- **Mô tả:** Tạo đánh giá cho đơn hàng
- **Authorization:** Required
- **Input:**
  ```json
  {
    "rating": 5,
    "comment": "Great product!"
  }
  ```

### 6.6 Lấy tracking đơn hàng
- **Path:** `GET /api/orders/{id}/tracking`
- **Mô tả:** Lấy thông tin tracking đơn hàng
- **Authorization:** Required
- **Output:** List<OrderTrackingDto>

### 6.7 Cập nhật trạng thái đơn hàng (Admin)
- **Path:** `PUT /api/orders/{id}/status`
- **Mô tả:** Cập nhật trạng thái đơn hàng
- **Authorization:** Admin required
- **Input:**
  ```json
  {
    "status": "shipped",
    "trackingNumber": "TRK123456"
  }
  ```

### 6.8 Cập nhật thông tin đơn hàng
- **Path:** `PUT /api/orders/{id}/update-info`
- **Mô tả:** Cập nhật thông tin đơn hàng
- **Authorization:** Required

---

## 7. Search APIs

### 7.1 Tìm kiếm sản phẩm
- **Path:** `GET /api/search/products`
- **Mô tả:** Tìm kiếm sản phẩm theo từ khóa
- **Query Parameters:**
  - `keyword`: Từ khóa tìm kiếm
- **Output:** List<ProductDto>

### 7.2 Lấy gợi ý tìm kiếm
- **Path:** `GET /api/search/suggestions`
- **Mô tả:** Lấy gợi ý tìm kiếm
- **Query Parameters:**
  - `keyword`: Từ khóa
- **Output:** List<SearchSuggestionDto>

---

## 8. Promotion APIs

### 8.1 Lấy tất cả khuyến mãi (Admin)
- **Path:** `GET /api/promotions`
- **Mô tả:** Lấy tất cả khuyến mãi
- **Authorization:** Admin required
- **Output:** List<PromotionDto>

### 8.2 Lấy khuyến mãi đang hoạt động
- **Path:** `GET /api/promotions/active`
- **Mô tả:** Lấy khuyến mãi đang hoạt động
- **Output:** List<PromotionDto>

### 8.3 Lấy chi tiết khuyến mãi (Admin)
- **Path:** `GET /api/promotions/{id}`
- **Mô tả:** Lấy thông tin chi tiết khuyến mãi
- **Authorization:** Admin required
- **Output:** PromotionDto

### 8.4 Tạo khuyến mãi mới (Admin)
- **Path:** `POST /api/promotions`
- **Mô tả:** Tạo khuyến mãi mới
- **Authorization:** Admin required
- **Input:**
  ```json
  {
    "name": "Promotion Name",
    "description": "Promotion description",
    "discountPercent": 20,
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "productIds": ["product1", "product2"]
  }
  ```

### 8.5 Cập nhật khuyến mãi (Admin)
- **Path:** `PUT /api/promotions/{id}`
- **Mô tả:** Cập nhật thông tin khuyến mãi
- **Authorization:** Admin required

### 8.6 Xóa khuyến mãi (Admin)
- **Path:** `DELETE /api/promotions/{id}`
- **Mô tả:** Xóa khuyến mãi
- **Authorization:** Admin required

---

## 9. Home APIs

### 9.1 Sản phẩm nổi bật
- **Path:** `GET /api/home/featured-products`
- **Mô tả:** Lấy sản phẩm nổi bật (có cache)
- **Output:** List<ProductDto>

### 9.2 Flash sale
- **Path:** `GET /api/home/flash-sale`
- **Mô tả:** Lấy sản phẩm flash sale (có cache)
- **Output:** List<ProductDto>

### 9.3 Danh mục
- **Path:** `GET /api/home/categories`
- **Mô tả:** Lấy danh mục (có cache)
- **Output:** List<CategoryDto>

### 9.4 Thống kê trang chủ
- **Path:** `GET /api/home/stats`
- **Mô tả:** Lấy thống kê trang chủ
- **Output:**
  ```json
  {
    "totalProducts": 1000,
    "totalCategories": 50,
    "totalOrders": 5000,
    "totalUsers": 2000
  }
  ```

---

## 10. Health Check APIs

### 10.1 Kiểm tra sức khỏe hệ thống
- **Path:** `GET /api/health`
- **Mô tả:** Kiểm tra trạng thái hệ thống
- **Output:**
  ```json
  {
    "status": "healthy",
    "timestamp": "2024-01-01T12:00:00Z",
    "mongodb": "connected",
    "environment": "Development"
  }
  ```

### 10.2 Kiểm tra chi tiết
- **Path:** `GET /api/health/detailed`
- **Mô tả:** Kiểm tra chi tiết các service
- **Output:**
  ```json
  {
    "status": "healthy",
    "timestamp": "2024-01-01T12:00:00Z",
    "environment": "Development",
    "mongodb": {
      "status": "connected",
      "connectionString": "configured",
      "database": "lecas"
    },
    "redis": {
      "status": "unknown",
      "connectionString": "configured"
    }
  }
  ```

---

## Error Codes

### HTTP Status Codes
- `200 OK`: Thành công
- `201 Created`: Tạo mới thành công
- `400 Bad Request`: Dữ liệu đầu vào không hợp lệ
- `401 Unauthorized`: Chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden`: Không có quyền truy cập
- `404 Not Found`: Không tìm thấy tài nguyên
- `500 Internal Server Error`: Lỗi server

### Error Response Format
```json
{
  "success": false,
  "message": "Error message",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

---

## Authentication Flow

1. **Đăng nhập:** Gọi `POST /api/auth/login` để lấy access token
2. **Sử dụng API:** Gửi token trong header `Authorization: Bearer <token>`
3. **Refresh Token:** Khi token hết hạn, gọi `POST /api/auth/refresh-token`
4. **Đăng xuất:** Gọi `POST /api/auth/logout` để vô hiệu hóa token

---

## Rate Limiting
- API có thể áp dụng rate limiting để tránh spam
- Giới hạn: 100 requests/phút cho mỗi IP
- Giới hạn: 1000 requests/giờ cho mỗi user

---

## Caching
- Một số API sử dụng Redis cache để tăng hiệu suất
- Cache được áp dụng cho: featured products, flash sale, categories
- Cache tự động expire theo cấu hình

---

## Notes
- Tất cả timestamps đều theo format ISO 8601
- IDs sử dụng MongoDB ObjectId format
- Images được lưu trữ dưới dạng URLs
- Hệ thống hỗ trợ đa ngôn ngữ (có thể mở rộng)
- API versioning có thể được thêm vào trong tương lai 