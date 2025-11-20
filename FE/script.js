const BASE_URL = 'https://shotten-be.taltiko.com';

const playerDropdown = document.getElementById('player-dropdown');
const matchesListSection = document.getElementById('matches-list');
const matchesContainer = document.getElementById('matches-container');

let selectedPlayerId = localStorage.getItem('selectedPlayerId') ? parseInt(localStorage.getItem('selectedPlayerId')) : null;
let players = [];

// Function to fetch players and populate the dropdown
async function loadPlayers() {
    console.log("loaiding players");
    try {
        const response = await fetch(`${BASE_URL}/api/Players`);
        players = await response.json();
        console.log('Loaded players:', players);

        playerDropdown.innerHTML = '<option value="">-- Please select a player --</option>';
        players.forEach(player => {
            const option = document.createElement('option');
            option.value = player.id;
            option.textContent = player.name;
            playerDropdown.appendChild(option);
        });

        if (selectedPlayerId) {
            playerDropdown.value = selectedPlayerId;
            matchesListSection.style.display = 'block';
            console.log('Selected player ID from localStorage:', selectedPlayerId);
            loadMatches();
        }
    } catch (error) {
        console.error('Error loading players:', error);
        alert('Failed to load players. Please ensure the API is running.');
    }
}

// Function to fetch matches and display them
async function loadMatches() {
    if (!selectedPlayerId) {
        matchesContainer.innerHTML = '<p>Please select a player to view matches.</p>';
        matchesListSection.style.display = 'none';
        return;
    }

    try {
        const response = await fetch(`${BASE_URL}/api/Matches`);
        const matches = await response.json();
        console.log('Loaded matches:', matches);

        // Sort matches by date ascending
        matches.sort((a, b) => {
            const dateA = new Date(a.date + (a.date && !a.date.endsWith('Z') ? 'Z' : ''));
            const dateB = new Date(b.date + (b.date && !b.date.endsWith('Z') ? 'Z' : ''));
            return dateA - dateB;
        });

        matchesContainer.innerHTML = '';
        if (matches.length === 0) {
            matchesContainer.innerHTML = '<p>No upcoming matches found.</p>';
            return;
        }

        let firstFutureMatchCard = null;
        const now = new Date();

        matches.forEach(match => {
            const matchCard = document.createElement('div');
            matchCard.classList.add('match-card');

            const playerAttendance = match.attendances.find(att => att.playerId === selectedPlayerId);

            // Group all players by their attendance status for this match
            const attendanceGroups = {
                Present: [],
                Maybe: [],
                NotPresent: [],
                Unknown: [] // For players not in the attendance list for this match
            };

            // Populate attendance groups from match.attendances
            match.attendances.forEach(att => {
                const player = players.find(p => p.id === att.playerId);
                if (player) {
                    attendanceGroups[att.status].push(player.name);
                }
            });

            // Add players who are not in the attendance list for this match to 'Unknown'
            players.forEach(player => {
                if (!match.attendances.some(att => att.playerId === player.id)) {
                    attendanceGroups.Unknown.push(player.name);
                }
            });

            // Generate HTML for attendance display
            let attendanceHtml = '';
            const orderedStatuses = ['Present', 'Maybe', 'NotPresent', 'Unknown'];

            attendanceHtml += `<div class="player-attendance-list">`;
            orderedStatuses.forEach(statusGroup => {
                attendanceGroups[statusGroup].forEach(playerName => {
                    const playerObj = players.find(p => p.name === playerName);
                    const isSelectedPlayer = playerObj && playerObj.id === selectedPlayerId;
                    const tileClasses = `player-attendance-item status-${statusGroup.toLowerCase()} ${isSelectedPlayer ? 'selected-player-tile' : ''}`;
                    const playerIdAttr = playerObj ? `data-player-id="${playerObj.id}"` : '';
                    const matchIdAttr = `data-match-id="${match.id}"`;
                    const currentStatusAttr = `data-current-status="${statusGroup}"`;
                    console.log(`Player: ${playerName}, ID: ${playerObj ? playerObj.id : 'N/A'}, Selected: ${isSelectedPlayer}, Classes: ${tileClasses}`);
                    attendanceHtml += `<span class="${tileClasses}" ${playerIdAttr} ${matchIdAttr} ${currentStatusAttr}>${playerName}</span>`;
                });
            });
            attendanceHtml += `</div>`;

            const locationLink = match.location
                ? `<a class="match-details" href="https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(match.location)}" target="_blank" rel="noopener noreferrer">${match.location}</a>`
                : `<p class="match-details">TBD</p>`;

            // Convert UTC date to local time string (force UTC parsing)
            let localDate;
            let matchDateObj;
            if (match.date) {
                matchDateObj = new Date(match.date + (match.date.endsWith('Z') ? '' : 'Z'));
                localDate = matchDateObj.toLocaleString(undefined, {
                    year: 'numeric',
                    month: 'short',
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                });
            } else {
                localDate = 'Unknown date';
            }

            // Identify the first future match card
            if (!firstFutureMatchCard && matchDateObj && matchDateObj > now) {
                firstFutureMatchCard = matchCard;
            }

            matchCard.innerHTML = `
                <h3 class="match-date-title">${localDate}</h3>
                <p class="match-details">${match.name}</p>
                ${locationLink}
                ${attendanceHtml}
            `;
            matchesContainer.appendChild(matchCard);
        });

        // Scroll to the first future match card if it exists
        if (firstFutureMatchCard) {
            // If the header is sticky, scrolling to the element's top will hide it behind the header.
            // Calculate an offset using the header height (if present) so the match is fully visible under the header.
            const headerEl = document.querySelector('header');
            const headerHeight = headerEl ? headerEl.getBoundingClientRect().height : 0;
            const elementTop = firstFutureMatchCard.getBoundingClientRect().top + window.scrollY;
            const scrollTo = Math.max(elementTop - headerHeight - 8, 0); // small 8px padding
            window.scrollTo({ top: scrollTo, behavior: 'smooth' });
        }

        // Add event listener for clicks on player tiles (delegated to matchesContainer)
        matchesContainer.removeEventListener('click', handlePlayerTileClick); // Remove previous listener to prevent duplicates
        matchesContainer.addEventListener('click', handlePlayerTileClick);

    } catch (error) {
        console.error('Error loading matches:', error);
        alert('Failed to load matches. Please ensure the API is running.');
    }
}

// Global variables for popup state
let currentMatchIdForPopup = null;
let currentPlayerIdForPopup = null;

// Get popup elements (moved inside DOMContentLoaded or init function)
let attendancePopup;
let popupOptionBtns;

// Function to show the popup
function showPopup(matchId, playerId) {
    currentMatchIdForPopup = matchId;
    currentPlayerIdForPopup = playerId;
    attendancePopup.style.display = 'flex'; // Use flex to center content
    console.log('Showing popup for Match ID:', matchId, 'Player ID:', playerId);
}

// Function to hide the popup
function hidePopup() {
    attendancePopup.style.display = 'none';
    currentMatchIdForPopup = null;
    currentPlayerIdForPopup = null;
    console.log('Hiding popup.');
}

// Handle clicks on player tiles
function handlePlayerTileClick(event) {
    const target = event.target;
    console.log('Clicked target:', target);
    if (target.classList.contains('selected-player-tile')) {
        const matchId = parseInt(target.dataset.matchId);
        const playerId = parseInt(target.dataset.playerId);
        console.log('Selected player tile clicked. Match ID:', matchId, 'Player ID:', playerId);
        showPopup(matchId, playerId);
    } else {
        console.log('Clicked element is not a selected-player-tile.');
    }
}

// Function to update attendance status
async function updateAttendance(matchId, playerId, status) {
    try {
        console.log('Updating attendance for Match ID:', matchId, 'Player ID:', playerId, 'Status:', status);
        const response = await fetch(`${BASE_URL}/api/matches/${matchId}/players/${playerId}/attendance?status=${status}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            console.log('Attendance updated successfully.');
            loadMatches(); // Reload matches to reflect the change
        } else {
            const errorText = await response.text();
            throw new Error(`Failed to update attendance: ${response.status} ${errorText}`);
        }
    } catch (error) {
        console.error('Error updating attendance:', error);
        alert('Error updating attendance: ' + error.message);
    }
}

// Event listener for player dropdown change
playerDropdown.addEventListener('change', (event) => {
    selectedPlayerId = parseInt(event.target.value);
    console.log('Player dropdown changed. New selected player ID:', selectedPlayerId);
    if (selectedPlayerId) {
        localStorage.setItem('selectedPlayerId', selectedPlayerId);
        matchesListSection.style.display = 'block';
        loadMatches();
    } else {
        localStorage.removeItem('selectedPlayerId');
        matchesListSection.style.display = 'none';
        matchesContainer.innerHTML = '<p>Please select a player to view matches.</p>';
    }
});

// Initialization function for popup and other DOM-dependent elements
function initializePopupAndListeners() {
    attendancePopup = document.getElementById('attendance-popup');
    popupOptionBtns = attendancePopup.querySelectorAll('.popup-option-btn');

    // Add event listeners to popup buttons
    popupOptionBtns.forEach(button => {
        button.addEventListener('click', async (event) => {
            const status = event.target.dataset.status;
            if (currentMatchIdForPopup && currentPlayerIdForPopup && status) {
                await updateAttendance(currentMatchIdForPopup, currentPlayerIdForPopup, status);
                hidePopup();
            }
        });
    });

    // Allow clicking outside the popup to close it
    attendancePopup.addEventListener('click', (event) => {
        if (event.target === attendancePopup) {
            hidePopup();
        }
    });

    // Player Management Popup
    const managePlayersBtn = document.getElementById('manage-players-btn');
    const managePlayersPopup = document.getElementById('manage-players-popup');
    const closeManagePlayersBtn = managePlayersPopup.querySelector('.close-popup-btn');
    const addPlayerBtn = document.getElementById('add-player-btn');
    const newPlayerNameInput = document.getElementById('new-player-name');

    managePlayersBtn.addEventListener('click', () => {
        loadPlayersForManagement();
        managePlayersPopup.style.display = 'flex';
    });

    closeManagePlayersBtn.addEventListener('click', () => {
        managePlayersPopup.style.display = 'none';
    });

    managePlayersPopup.addEventListener('click', (event) => {
        if (event.target === managePlayersPopup) {
            managePlayersPopup.style.display = 'none';
        }
    });

    addPlayerBtn.addEventListener('click', async () => {
        const newName = newPlayerNameInput.value.trim();
        if (newName) {
            try {
                const response = await fetch(`${BASE_URL}/api/Players`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ name: newName }),
                });
                if (response.ok) {
                    newPlayerNameInput.value = '';
                    loadPlayersForManagement();
                    loadPlayers(); // Refresh main player dropdown
                } else {
                    throw new Error('Failed to add player');
                }
            } catch (error) {
                console.error('Error adding player:', error);
            }
        }
    });
}

async function loadPlayersForManagement() {
    try {
        const response = await fetch(`${BASE_URL}/api/Players`);
        const players = await response.json();

        const playerListContainer = document.getElementById('player-list-container');
        playerListContainer.innerHTML = '';
        players.forEach(player => {
            const playerItem = document.createElement('div');
            playerItem.classList.add('player-item');
            playerItem.innerHTML = `
                <span class="player-name">${player.name}</span>
                <div class="player-actions">
                    <button class="edit-player-btn icon-btn" data-player-id="${player.id}">✏️</button>
                    <button class="delete-player-btn icon-btn" data-player-id="${player.id}">🗑️</button>
                </div>
            `;
            playerListContainer.appendChild(playerItem);
        });

        // Add event listeners for edit and delete buttons
        playerListContainer.querySelectorAll('.edit-player-btn').forEach(button => {
            button.addEventListener('click', handleEditPlayer);
        });
        playerListContainer.querySelectorAll('.delete-player-btn').forEach(button => {
            button.addEventListener('click', handleDeletePlayer);
        });

    } catch (error) {
        console.error('Error loading players for management:', error);
    }
}

async function handleEditPlayer(event) {
    const playerId = event.target.dataset.playerId;
    const playerItem = event.target.closest('.player-item');
    const playerNameSpan = playerItem.querySelector('.player-name');
    const currentName = playerNameSpan.textContent;

    const newName = prompt('Enter new name:', currentName);
    if (newName && newName.trim() !== currentName) {
        try {
            const response = await fetch(`${BASE_URL}/api/Players/${playerId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id: playerId, name: newName.trim() }),
            });
            if (response.ok) {
                loadPlayersForManagement();
                loadPlayers(); // Refresh main player dropdown
            } else {
                throw new Error('Failed to update player');
            }
        } catch (error) {
            console.error('Error updating player:', error);
        }
    }
}

async function handleDeletePlayer(event) {
    const playerId = event.target.dataset.playerId;
    if (confirm('Are you sure you want to delete this player?')) {
        try {
            const response = await fetch(`${BASE_URL}/api/Players/${playerId}`, {
                method: 'DELETE',
            });
            if (response.ok) {
                loadPlayersForManagement();
                loadPlayers(); // Refresh main player dropdown
            } else {
                throw new Error('Failed to delete player');
            }
        } catch (error) {
            console.error('Error deleting player:', error);
        }
    }
}

// Initial load
document.addEventListener('DOMContentLoaded', () => {
    loadPlayers();
    initializePopupAndListeners();
});


