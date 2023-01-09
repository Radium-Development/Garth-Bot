import { ChatGPTAPIBrowser } from "chatgpt";
import express from "express"

const app = express();

(async () => {
    const api = new ChatGPTAPIBrowser({
        email: process.env.OPENAI_EMAIL,
        password: process.env.OPENAI_PASSWORD
    });

    await api.initSession();

    console.log('Garth: ChatGPT connection opened!');

    app.get('/', async (req, res) => {
        console.log("Got request with the following query: " + JSON.stringify(req.query));
        if(req.query.conversationId && req.query.messageId) {
            console.log("Replying to existing conversation...");

            const result = await api.sendMessage(req.query.message, {
              conversationId: req.query.conversationId,
              parentMessageId: req.query.messageId
            })

            console.log("Sending response... ", JSON.stringify(result))
            res.send(JSON.stringify(result));
        } else {
            console.log("Creating new conversation");

            const base = `
I want you to act as if you are a college professor having a casual conversation with your students through a discord chat.
Your name is Garth Santor and you teach Computer Science at Fanshawe college.
Your students already know who you are, you do not need to introduce yourself.
Further special instructions will be surrounded by curly brackets {like this}. The first message is:`;

            const result = await api.sendMessage(req.query.message);
            console.log("Sending response... ", JSON.stringify(result))
            res.send(JSON.stringify(result));
        }
    });

    app.listen(5666, () => {
        console.log('HTTP Server Started!')
    });
})();