import { useState } from 'react'
import { askChatbot } from '../../api/chatbot'
import { useAuth } from '../../context/AuthContext'
import './ChatWidget.css'

function ChatWidget() {
    const { auth } = useAuth()

    // Only show for students or unauthenticated users
    if (auth && auth.role !== 'Student') {
        return null
    }
    const [isOpen, setIsOpen] = useState(false)
    const [messages, setMessages] = useState([])
    const [input, setInput] = useState('')
    const [isLoading, setIsLoading] = useState(false)

    const toggleChat = () => {
        setIsOpen(!isOpen)
    }

    const handleSend = async () => {
        if (!input.trim()) return

        const userMessage = input.trim()
        setInput('')
        setMessages(prev => [...prev, { type: 'user', text: userMessage }])
        setIsLoading(true)

        try {
            const response = await askChatbot(userMessage)
            
            const botAnswer = response.answer || response.Answer || 'Nemam odgovor na to pitanje.'
            
            setMessages(prev => [...prev, { type: 'bot', text: botAnswer }])
        } catch (error) {
            setMessages(prev => [...prev, { type: 'bot', text: 'Došlo je do greške. Pokušajte ponovo.' }])
        } finally {
            setIsLoading(false)
        }
    }

    const handleKeyPress = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault()
            handleSend()
        }
    }

    return (
        <div className="chat-widget">
            <button 
                className="chat-toggle-btn"
                onClick={toggleChat}
                aria-label="Chat"
            >
                💬
            </button>

            {isOpen && (
                <div className="chat-window">
                    <div className="chat-header">
                        <h3>Pomoćnik</h3>
                        <button 
                            className="close-btn"
                            onClick={toggleChat}
                            aria-label="Zatvori"
                        >
                            ✕
                        </button>
                    </div>

                    <div className="chat-messages">
                        {messages.length === 0 && (
                            <div className="welcome-message">
                                <p>Postavite pitanje o studentskom domu!</p>
                            </div>
                        )}
                        {messages.map((msg, index) => (
                            <div 
                                key={index} 
                                className={`message ${msg.type === 'user' ? 'user-message' : 'bot-message'}`}
                            >
                                {msg.text}
                            </div>
                        ))}
                        {isLoading && (
                            <div className="message bot-message">
                                <span className="typing-indicator">...</span>
                            </div>
                        )}
                    </div>

                    <div className="chat-input">
                        <textarea
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            onKeyPress={handleKeyPress}
                            placeholder="Unesite pitanje..."
                            rows={2}
                            disabled={isLoading}
                        />
                        <button 
                            className="send-btn"
                            onClick={handleSend}
                            disabled={isLoading || !input.trim()}
                        >
                            Pošalji
                        </button>
                    </div>
                </div>
            )}
        </div>
    )
}

export default ChatWidget
