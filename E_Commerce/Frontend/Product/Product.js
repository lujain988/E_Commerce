const itemsPerPage = 6; // Number of items per page
let currentPage = 1; // Current page number
let products = []; // Store all products

async function GetAllProduct() {
  let url = "https://localhost:7222/api/Product";
  let request = await fetch(url);
  products = await request.json();

  // Render the products for the current page
  renderPage(currentPage);
}

function renderPage(page) {
  const startIndex = (page - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const pageProducts = products.slice(startIndex, endIndex);

  let cards = document.getElementById("conteainer");
  cards.innerHTML = "";

  pageProducts.forEach((product) => {
    let imageUrl = `${product.image}`;
    let cardHTML = `
            <div class="col-lg-4 col-md-6 text-center">
                <div class="single-product-item">
                    <div class="product-image">
                        <a href="single-product.html"><img src="${imageUrl}" alt=""></a>
                    </div>
                    <h3>${product.productName}</h3>
                    <p class="product-price"> ${product.price}$</p>
                    <div class="product-rating" id="rating-${product.id}">
                        <!-- Rating will be inserted here -->
                    </div>
                    <a href="cart.html" class="cart-btn"><i class="fas fa-shopping-cart"></i> Add to Cart</a>
                    <br>
                    <a href="single-product.html">
                        <i class="fas fa-chevron-right"> see more </i>
                    </a>
                </div>
            </div>
        `;
    cards.innerHTML += cardHTML;
    // Fetch and update rating for each product
    fetchAverageRating(product.id);
  });

  // Update pagination controls
  updatePaginationControls();
}

function updatePaginationControls() {
  const totalPages = Math.ceil(products.length / itemsPerPage);
  let pagination = document.getElementById("pagination");
  pagination.innerHTML = "";

  let paginationHTML = `
        <ul>
            <li ${
              currentPage === 1 ? 'class="disabled"' : ""
            }><a href="#" onclick="changePage(${currentPage - 1})">Prev</a></li>
    `;

  for (let i = 1; i <= totalPages; i++) {
    paginationHTML += `
            <li ${
              i === currentPage ? 'class="active"' : ""
            }><a href="#" onclick="changePage(${i})">${i}</a></li>
        `;
  }

  paginationHTML += `
            <li ${
              currentPage === totalPages ? 'class="disabled"' : ""
            }><a href="#" onclick="changePage(${currentPage + 1})">Next</a></li>
        </ul>
    `;

  pagination.innerHTML = paginationHTML;
}

function changePage(page) {
  if (page < 1 || page > Math.ceil(products.length / itemsPerPage)) return;
  currentPage = page;
  renderPage(currentPage);
}

async function fetchAverageRating(productId) {
  let url = `https://localhost:7222/api/Product/average-rating/${productId}`;
  let response = await fetch(url);
  let data = await response.json();

  let averageRating =
    data && data.averageRating != null ? data.averageRating : 5;

  let ratingContainer = document.getElementById(`rating-${productId}`);

  ratingContainer.innerHTML = `<i class="fas fa-star"> Rating: ${averageRating}</i>`;
}

GetAllProduct();
