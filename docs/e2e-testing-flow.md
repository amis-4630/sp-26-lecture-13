# End-to-End Testing Scenarios

## Flow 1: New User Registration

1. Navigate to Register page
2. Enter email, password (meets complexity rules)
3. Submit → account created → redirected to login or auto-logged in
4. Verify: can you log in with the new account?

## Flow 2: Shopping Flow (as Customer)

1. Log in as Customer
2. Browse products → add to cart
3. View cart → update quantity → remove item → verify totals
4. Proceed to checkout → enter shipping address → place order
5. Verify: cart is cleared after order placement
6. View order history → order appears with correct details

## Flow 3: Admin Operations\*\*

1. Log in as Admin (provide test credentials in submission)
2. Access admin dashboard
3. Add a new product → verify it appears in product list
4. Edit a product → verify changes persist
5. View all orders → update order status
6. Verify: a Customer account CANNOT access admin endpoints

## Flow 4: Security Checks\*\*

1. Access a protected page without logging in → redirected to login
2. Try accessing `/api/orders` without a token → 401 response
3. Try accessing admin endpoints as Customer → 403 response
