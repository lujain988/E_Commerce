async function getAllProduct() {
    const productId = localStorage.getItem("products");
    const url = `https://localhost:7222/api/Product/${productId}`;

    let request = await fetch(url);
    let data = await request.json();
    let cards = document.getElementById("productCard");

    cards.innerHTML = `
        <div class="col-md-5">
            <div class="single-product-img">
                <img src="${data.image}" alt="">
            </div>
        </div>
        <div class="col-md-7">
            <div class="single-product-content">
                <h3>${data.productName}</h3>
                <p class="single-product-pricing"> ${data.price}$</p>
                <p>${data.description}</p>
                <div class="single-product-form">
                    <form >
                        <input type="number" id="quantity" value="1">
                    </form>
                    <a href="cart.html" class="cart-btn"><i class="fas fa-shopping-cart"></i> Add to Cart</a>
                    <p><strong>Category: ${data.categoryName}</strong></p>
                </div>
                <h4>Share:</h4>
                <ul class="product-share">
                    <li><a href="#" onclick="shareToFacebook(${data.id})"><i class="fab fa-facebook-f"></i></a></li>
                    <!-- Add other share buttons if needed -->
                </ul>
              
                <a href="Product.html" class="back-to-shopping" style="color: #ff8c00;"><i class="fas fa-arrow-left" ></i> Back to Shopping</a>
            </div>
        </div>
    `;

    console.log(data);
}

function shareToFacebook(productId) {
    const url = `https://localhost:7222/api/Product/${productId}`;
    const facebookShareURL = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`;
    window.open(facebookShareURL, 'facebook-share-dialog', 'width=800,height=600');
}

getAllProduct();
