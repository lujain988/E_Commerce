// Variables to handle pagination
let currentPage = 1;
const itemsPerPage = 5;
let productId = 1; // Default or replace with dynamic value

// Fetch reviews for the given productId
async function fetchReviews(productId) {
  const url = `https://localhost:7222/GetAllReview?productId=${productId}`; // Ensure endpoint supports filtering by productId
  try {
    const response = await fetch(url);
    const reviews = await response.json();
    
    console.log(reviews); // Check the fetched data
    
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

  paginatedItems.forEach(review => {
    let actionButtons = '';

    // Add buttons for approval if status is not approved
    if (review.status !== 'Approved' && review.status !== 'Declined') {
      actionButtons += `
        <button class="btn btn-success" onclick="approveReview(${review.id})">Approve</button>
        <button class="btn btn-secondary" onclick="declineReview(${review.id})">Decline</button>
      `;
    }
    
    // Add the delete button last
    actionButtons += `
      <button class="btn btn-danger" onclick="deleteReview(${review.id})">Delete</button>
    `;

    // Add table row with conditional buttons
    reviewContainer.innerHTML += `
      <tr>
        <td>${review.user}</td>
        <td>${review.productName}</td>
        <td>${review.categoryName}</td>
        <td>${review.comment}</td>
        <td>${review.status}</td>
        <td>
          ${actionButtons}
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
  fetchReviews(productId);  // Re-fetch and display items for the new page
}

fetchReviews(productId);

function getCategoryId(categoryId) {
  localStorage.setItem('categoryid', categoryId);
  window.location.href = 'UpdataCategory.html';
}

async function deleteReview(reviewId) {
  if (confirm('Are you sure you want to delete this review?')) {
    try {
      const response = await fetch(`https://localhost:7222/api/Product/${reviewId}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        alert('Review deleted successfully');
        fetchReviews(productId);
      } else {
        alert('Failed to delete review');
      }
    } catch (error) {
      console.error('Error deleting review:', error);
      alert('Error deleting review');
    }
  }
}

async function approveReview(reviewId) {
  if (confirm('Are you sure you want to approve this review?')) {
    try {
      const response = await fetch(`https://localhost:7222/Admin/${reviewId}`, {
        method: 'PUT',  
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          status: 'Approved'  
        }),
      });

      if (response.ok) {
        alert('Review approved successfully');
        fetchReviews(productId); 
      } else {
        alert('Failed to approve review');
      }
    } catch (error) {
      console.error('Error approving review:', error);
      alert('Error approving review');
    }
  }
}


async function declineReview(reviewId) {
  if (confirm('Are you sure you want to decline this review?')) {
    try {
      const response = await fetch(`https://localhost:7222/Admin/${reviewId}`, {
        method: 'PUT',  
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          status: 'Declined'
        }),
      });

      if (response.ok) {
        alert('Review declined successfully');
        fetchReviews(productId); 
      } else {
        alert('Failed to declined review');
      }
    } catch (error) {
      console.error('Error approving review:', error);
      alert('Error approving review');
    }
  }
}