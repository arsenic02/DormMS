import { useState } from 'react'
import { askChatbot } from '../../api/chatbot'
import { useAuth } from '../../context/AuthContext'
import styles from './ChatWidget.module.css'
// import './ChatWidget.css'

function ChatWidget() {
    const { auth } = useAuth()

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

            //za 1 najslicniji odgovor
            //const botAnswer = response.answer || response.Answer || 'Nemam odgovor na to pitanje.'

            if (Array.isArray(response) && response.length > 0) {
                setMessages(prev => [...prev, { type: 'bot', results: response }])
            } else {
                const fallbackText = response.message || 'Nemam odgovor na to pitanje.'
                setMessages(prev => [...prev, { type: 'bot', text: fallbackText }])
            }

            //setMessages(prev => [...prev, { type: 'bot', text: botAnswer }])
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
        <div className={styles.chatWidget}>
            <button
                className={styles.chatToggleBtn}
                onClick={toggleChat}
                aria-label="Chat"
            >
                💬
            </button>

            {isOpen && (
                <div className={styles.chatWindow}>
                    <div className={styles.chatHeader}>
                        <h3>AI asistent</h3>
                        <button
                            className={styles.closeBtn}
                            onClick={toggleChat}
                            aria-label="Zatvori"
                        >
                            ✕
                        </button>
                    </div>

                    <div className={styles.chatMessages}>
                        {messages.length === 0 && (
                            <div className={styles.welcomeMessage}>
                                <p>Postavite pitanje o studentskom domu!</p>
                            </div>
                        )}
                        {messages.map((msg, index) => (
                            <div
                                key={index}
                                className={`message ${msg.type === 'user' ? styles.userMessage : styles.botMessage}`}
                            >
                                {msg.results ? (
                                    <div className={styles.botResults}>
                                        {msg.results.map((result, i) => (
                                            <div key={i} className={styles.resultItem}>
                                                <p className={styles.resultQuestion}>{result.question}</p>
                                                <p className={styles.resultAnswer}>{result.answer}</p>
                                            </div>
                                        ))}
                                    </div>
                                ) : (
                                    msg.text
                                )}
                                {/* {msg.text} */}
                            </div>
                        ))}
                        {isLoading && (
                            <div className={`message ${styles.botMessage}`}>
                                <span className={styles.typingIndicator}>...</span>
                            </div>
                        )}
                    </div>

                    <div className={styles.chatInput}>
                        <textarea
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            onKeyPress={handleKeyPress}
                            placeholder="Unesite pitanje..."
                            rows={2}
                            disabled={isLoading}
                        />
                        <button
                            className={styles.sendBtn}
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
