const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors()); // Allows your mod menu to talk to this server
app.use(bodyParser.json());

// Serve static files (Your sounds/images)
// Put your "Resources" folder inside the "public" folder on your server
app.use('/assets', express.static(path.join(__dirname, 'public')));

// --- Endpoints ---

// 1. Get Friends List
app.get('/getfriends', (req, res) => {
    // This is a placeholder. You can connect a database here later!
    res.json({
        friends: {
            "USER_ID_EXAMPLE": {
                "currentUserID": "USER_ID_EXAMPLE",
                "displayName": "Quantum Dev",
                "pfpURL": "https://i.imgur.com/example.png"
            }
        },
        incoming: {},
        outgoing: {}
    });
});

// 2. Text to Speech (Placeholder)
app.post('/tts', (req, res) => {
    const { text, voice } = req.body;
    console.log(`TTS Request: [${voice}] ${text}`);
    // You would integrate a TTS library here
    res.status(501).send("TTS Not implemented yet");
});

// 3. Social Actions
app.post('/frienduser', (req, res) => {
    const { target } = req.body;
    res.json({ status: "success", message: `Friend request sent to ${target}` });
});

// Home Page
app.get('/', (req, res) => {
    res.send("<h1>Quantum Menu Backend is Online!</h1>");
});

app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});
