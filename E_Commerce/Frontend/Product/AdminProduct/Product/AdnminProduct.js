let currentPage = 1;
const itemsPerPage = 5;


async function getReviews() {
    try {
        const response = await fetch('https://localhost:7222/Reviews/'); // Fetch all reviews
        const reviews = await response.json();

        displayPaginatedReviews(reviews, currentPage, itemsPerPage);
        setupPagination(reviews, itemsPerPage);
        
    } catch (error) {
        console.error('Error fetching reviews:', error);
    }
}

// Function to display paginated reviews
function displayPaginatedReviews(reviews, page, itemsPerPage) {
    const reviewContainer = document.getElementById('container');
    reviewContainer.innerHTML = '';  // Clear the current items

    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const paginatedItems = reviews.slice(start, end);

    // Create table rows for paginated items
    paginatedItems.forEach(review => {
        reviewContainer.innerHTML += `
            <tr>
                <td>${review.id}</td>
                <td>${review.user}</td>
                <td>${review.productName}</td>
                <td>${review.categoryName}</td>
                <td>${review.comment}</td>
                <td>${review.rating ?? 'N/A'}</td>
                <td>${review.status}</td>
                <td>
                    <button class="btn btn-warning" onclick="editReview(${review.id})">Edit</button>
                    <button class="btn btn-danger" onclick="deleteReview(${review.id})">Delete</button>
                </td>
            </tr>
        `;
    });
}
// Function to set up pagination controls
function setupPagination(items, itemsPerPage) {
    const paginationContainer = document.getElementById('pagination-container');
    paginationContainer.innerHTML = '';  // Clear current pagination

    const pageCount = Math.ceil(items.length / itemsPerPage);
    
    for (let i = 1; i <= pageCount; i++) {
        paginationContainer.innerHTML += `<button class="btn btn-secondary mx-1" onclick="changePage(${i})">${i}</button>`;
    }
}

// Function to handle page changes
function changePage(pageNumber) {
    currentPage = pageNumber;
    getReviews();  // Re-fetch and display items for the new page
}
 
// Fetch and display the first set of reviews
getReviews();
