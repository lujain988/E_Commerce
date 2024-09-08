
async function fetchAndDisplayCategories() {
    try {
        const response = await fetch('https://localhost:7222/api/Categories/GetAllCategory');
        const categories = await response.json();

        // // Filter specific categories you want to display by categoryName
        // const filteredCategories = categories.filter(category => 
        //     category.categoryName === "Pen and Pencils" ||
           
        //     category.categoryName === "Notebooks" ||
        //     category.categoryName === "Bags" 
          
        // );

        // Limit the filtered categories to 4 if needed
        const limitedCategories = categories.slice(0, 3);
        const categoryContainer = document.getElementById('category-container');

        // Build the HTML for all categories in one go
        let categoriesHTML = '';
        limitedCategories.forEach(category => {
            categoriesHTML += `
                <div class="col-lg-3 col-md-6 text-center category-card">
                    <div class="category-image">
                        <img src="../../Uploads/${category.image}" alt="${category.categoryName}">
                    </div>
                    <h3>${category.categoryName}</h3>
                    <a href="" onclick="saveToLocalStorage(${category.id} class="view-all-btn">View All</a>
                </div>
            `;
        });

        // Set the inner HTML of the container to the generated HTML
        categoryContainer.innerHTML = categoriesHTML;
        
    } catch (error) {
        console.error('Error fetching categories:', error);
    }
}

function saveToLocalStorage(id) {
    localStorage.setItem("CategoryId", id);
    window.location.href = "../../../Product/Product.html";
  }
// Call the function to fetch and display categories
fetchAndDisplayCategories();



async function fetchAndDisplayDiscountProducts() {
    try {
        const response = await fetch('https://localhost:7222/api/Product/discounted');
        const products = await response.json();

        const discountProductContainer = document.getElementById('discount-product-container');
        let productsHTML = '';

        // Limit the products to the first 3
        const limitedProducts = products.slice(0, 3);

        // Loop through the limited discounted products and build HTML for each
        limitedProducts.forEach(product => {
            const discountedPrice = product.price - (product.price * (product.discount / 100));
            productsHTML += `
                <div class="col-lg-4 col-md-6 text-center">
                    <div class="single-product-item">
                        <div class="product-image">
                            <a href="single-product.html">
                                <img src="../../Uploads/${product.image}" alt="${product.productName}">
                            </a>
                            <span class="discount-badge">${product.discount}% Off</span>
                        </div>
                        <h3>${product.productName}</h3>
                        <p class="product-price"><span class="original-price">$${product.price.toFixed(2)}</span> $${discountedPrice.toFixed(2)}</p>
                        <a href="cart.html" class="cart-btn"><i class="fas fa-shopping-cart"></i> Add to Cart</a>
                    </div>
                </div>
            `;
        });

        discountProductContainer.innerHTML = productsHTML;
    } catch (error) {
        console.error('Error fetching discounted products:', error);
    }
}


// Call the function to fetch and display discount products
fetchAndDisplayDiscountProducts();
