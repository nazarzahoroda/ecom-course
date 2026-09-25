#!/bin/bash
set -e

echo "1. Оновлення CustomerStore -> CustomerRepository..."
find src tests -type f -name "*.cs" -exec sed -i 's/ICustomerStore/ICustomerRepository/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/CustomerStore/CustomerRepository/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/customerStore/customerRepository/g' {} +

echo "2. Оновлення JwtService -> TokenIssuer..."
find src tests -type f -name "*.cs" -exec sed -i 's/IJwtService/ITokenIssuer/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/JwtService/JwtTokenIssuer/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/jwtService/tokenIssuer/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/EcomCourse.Application.Authentication.Interfaces/EcomCourse.Application.Abstractions.Authentication/g' {} +

echo "3. Оновлення IdentityService -> IdentityProvider..."
find src tests -type f -name "*.cs" -exec sed -i 's/IIdentityService/IIdentityProvider/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/IdentityService/IdentityProvider/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/identityService/identityProvider/g' {} +

echo "4. Оновлення CartService -> CartManager..."
find src tests -type f -name "*.cs" -exec sed -i 's/ICartService/ICartManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/CartService/CartManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/cartService/cartManager/g' {} +

echo "5. Оновлення ProductService -> ProductManager..."
find src tests -type f -name "*.cs" -exec sed -i 's/IProductService/IProductManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/ProductService/ProductManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/productService/productManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/EcomCourse.Application.Products.Services/EcomCourse.Application.Abstractions/g' {} +

echo "6. Оновлення CategoryService -> CategoryManager..."
find src tests -type f -name "*.cs" -exec sed -i 's/ICategoryService/ICategoryManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/CategoryService/CategoryManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/categoryService/categoryManager/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/EcomCourse.Application.Categories.Services/EcomCourse.Application.Abstractions/g' {} +

echo "7. Оновлення неймспейсів Interfaces -> Abstractions..."
find src tests -type f -name "*.cs" -exec sed -i 's/namespace EcomCourse.Application.Interfaces/namespace EcomCourse.Application.Abstractions/g' {} +
find src tests -type f -name "*.cs" -exec sed -i 's/using EcomCourse.Application.Interfaces/using EcomCourse.Application.Abstractions/g' {} +

echo "8. Перенесення файлів..."
mv src/EcomCourse.Domain/Customers/ICustomerStore.cs src/EcomCourse.Domain/Customers/ICustomerRepository.cs
mv src/EcomCourse.Infrastructure/Customers/CustomerStore.cs src/EcomCourse.Infrastructure/Customers/CustomerRepository.cs

mkdir -p src/EcomCourse.Application/Abstractions/Authentication

mv src/EcomCourse.Application/Authentication/Interfaces/IJwtService.cs src/EcomCourse.Application/Abstractions/Authentication/ITokenIssuer.cs
mv src/EcomCourse.Application/Interfaces/IIdentityService.cs src/EcomCourse.Application/Abstractions/IIdentityProvider.cs
mv src/EcomCourse.Application/Interfaces/IUserContext.cs src/EcomCourse.Application/Abstractions/IUserContext.cs
mv src/EcomCourse.Application/Interfaces/ICartService.cs src/EcomCourse.Application/Abstractions/ICartManager.cs
mv src/EcomCourse.Application/Products/Services/IProductService.cs src/EcomCourse.Application/Abstractions/IProductManager.cs
mv src/EcomCourse.Application/Categories/Services/ICategoryService.cs src/EcomCourse.Application/Abstractions/ICategoryManager.cs

mv src/EcomCourse.Infrastructure/Persistence/Identity/JwtService.cs src/EcomCourse.Infrastructure/Persistence/Identity/JwtTokenIssuer.cs
mv src/EcomCourse.Infrastructure/Services/IdentityService.cs src/EcomCourse.Infrastructure/Services/IdentityProvider.cs
mv src/EcomCourse.Infrastructure/Services/CartService.cs src/EcomCourse.Infrastructure/Services/CartManager.cs
mv src/EcomCourse.Infrastructure/Services/ProductService.cs src/EcomCourse.Infrastructure/Services/ProductManager.cs
mv src/EcomCourse.Infrastructure/Services/CategoryService.cs src/EcomCourse.Infrastructure/Services/CategoryManager.cs

echo "9. Очищення порожніх папок..."
rmdir src/EcomCourse.Application/Interfaces 2>/dev/null || true
rmdir src/EcomCourse.Application/Authentication/Interfaces 2>/dev/null || true
rmdir src/EcomCourse.Application/Products/Services 2>/dev/null || true
rmdir src/EcomCourse.Application/Categories/Services 2>/dev/null || true

echo "10. Додавання правила в review-rules.md..."
sed -i '/## Suggestion (comment, non-blocking)/i \
10. **Infrastructure Port Interfaces** — Persistence ports (repositories) must be named `I*Repository` and placed in `Domain//` with no framework/DTO dependencies. Other infrastructure capabilities (clock, token issuance, etc.) must be named for their capability (e.g. `ITokenIssuer`, not `IJwtService`) and placed in `Application/Abstractions/`. No `I*Service` port names are allowed.\
' docs/review-rules.md

echo "Рефакторинг успішно завершено! 🚀"
