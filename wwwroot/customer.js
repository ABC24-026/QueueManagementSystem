async function joinQueue() {
    const phone = document.getElementById('phone').value.trim();
    const serviceType = document.getElementById('serviceType').value;
    const resultDiv = document.getElementById('result');

    if (!phone) {
        showResult('Please enter your phone number', false);
        return;
    }

    try {
        const response = await fetch('/api/tickets/join', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ phone, serviceType })
        });

        const data = await response.json();

        if (response.ok) {
            showResult(`
                <p>You have joined the queue!</p>
                <div class="ticket-number">${data.ticketNumber}</div>
                <p>We will send you an SMS when your turn is near.</p>
            `, true);
        } else {
            showResult(data.error || 'Something went wrong', false);
        }
    } catch (err) {
        showResult('Could not connect to the server', false);
    }
}

function showResult(message, isSuccess) {
    const resultDiv = document.getElementById('result');
    resultDiv.innerHTML = message;
    resultDiv.className = 'result ' + (isSuccess ? 'success' : 'error');
    resultDiv.classList.remove('hidden');
}