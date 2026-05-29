using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public class KeywordResponder
    {
        private Dictionary<string, List<string>> _responses;
        private Random _random = new Random();

        public KeywordResponder()
        {
            _responses = new Dictionary<string, List<string>>
            {
                {
                    "password", new List<string>
                    {
                        "Use at least 12 characters — mix uppercase, lowercase, numbers, and symbols.",
                        "Never reuse the same password across multiple accounts.",
                        "Consider a reputable password manager to generate and store strong passwords.",
                        "Avoid using personal details like your name or birthday in passwords.",
                        "Enable two-factor authentication (2FA) alongside strong passwords for extra security."
                    }
                },
                {
                    "phishing", new List<string>
                    {
                        "Be cautious of emails asking for personal information — scammers disguise themselves as trusted organisations.",
                        "Check the sender's actual email address, not just the display name.",
                        "Never click a link in an unsolicited email — type the URL directly into your browser.",
                        "Hover over links before clicking to reveal the real destination URL.",
                        "Legitimate banks will NEVER ask for your password via email."
                    }
                },
                {
                    "scam", new List<string>
                    {
                        "If an offer sounds too good to be true, it almost certainly is.",
                        "Never send money or gift cards to someone you have only met online.",
                        "Verify unexpected prize winnings through official channels before acting.",
                        "Scammers create urgency — slow down and think before acting on any alarming message.",
                        "Report suspected scams to your national cybercrime authority to protect others."
                    }
                },
                {
                    "privacy", new List<string>
                    {
                        "Review app permissions regularly — many apps request access they do not need.",
                        "Use a VPN on public Wi-Fi to encrypt your traffic and protect your data.",
                        "Check your social media privacy settings so only trusted contacts can see your info.",
                        "Be mindful of what you post — personal details can be exploited by attackers.",
                        "Read privacy policies before signing up for new services."
                    }
                },
                {
                    "malware", new List<string>
                    {
                        "Use reputable antivirus software and keep it updated at all times.",
                        "Never download files or software from unknown or untrusted sources.",
                        "Keep your operating system and apps updated to patch security holes.",
                        "Avoid pirated software — it is a common delivery method for malware.",
                        "Back up your data regularly so ransomware cannot permanently destroy your files."
                    }
                },
                {
                    "hacking", new List<string>
                    {
                        "Most successful hacks exploit weak passwords or human error, not technical wizardry.",
                        "Keep all software and firmware updated — patches fix known vulnerabilities.",
                        "Use network monitoring tools to detect unusual activity on your devices.",
                        "Enable a firewall on your router and device for an added layer of protection.",
                        "Be sceptical of unsolicited messages — social engineering is a top hacking tactic."
                    }
                },
                {
                    "wifi", new List<string>
                    {
                        "Public Wi-Fi is a hunting ground for attackers — always use a VPN on public networks.",
                        "Avoid accessing banking or email on public Wi-Fi.",
                        "Forget Wi-Fi networks you no longer use to prevent automatic reconnection.",
                        "Use WPA3 encryption on your home router if supported.",
                        "Turn off Wi-Fi and Bluetooth when not in use to reduce your attack surface."
                    }
                },
                {
                    "2fa", new List<string>
                    {
                        "Two-factor authentication adds a second verification step beyond your password.",
                        "Even if someone steals your password, 2FA stops them without the second factor.",
                        "Enable 2FA on your email, banking, and social media accounts immediately.",
                        "Use an authenticator app like Google Authenticator instead of SMS when possible.",
                        "Never share your 2FA code with anyone — no legitimate service will ever ask for it."
                    }
                },
                {
                    "vpn", new List<string>
                    {
                        "A VPN encrypts your internet traffic and hides your IP address from third parties.",
                        "Use a VPN when on public Wi-Fi to prevent eavesdropping on your connection.",
                        "Choose a reputable, no-log VPN provider to ensure your data is not stored or sold.",
                        "A VPN does not make you fully anonymous — combine it with good browsing habits.",
                        "VPNs are useful for accessing region-restricted content securely while travelling."
                    }
                },
                {
                    "ransomware", new List<string>
                    {
                        "Back up your data regularly — offline or cloud backups are your best defence.",
                        "Never open unexpected email attachments, even from known contacts.",
                        "Keep your OS and software patched — ransomware exploits known vulnerabilities.",
                        "Do not pay the ransom — there is no guarantee your files will be restored.",
                        "Use endpoint protection software to detect and block ransomware before it runs."
                    }
                }
            };
        }

        public string GetResponse(string input, out string matchedKeyword)
        {
            matchedKeyword = null;
            string lower = input.ToLower();

            if (lower.Contains("hack")) lower = lower.Replace("hack", "hacking");
            if (lower.Contains("virus")) lower = lower.Replace("virus", "malware");
            if (lower.Contains("network")) lower = lower.Replace("network", "wifi");
            if (lower.Contains("authenticat")) lower = lower.Replace("authenticat", "2fa");

            foreach (var kvp in _responses)
            {
                if (lower.Contains(kvp.Key))
                {
                    matchedKeyword = kvp.Key;
                    return kvp.Value[_random.Next(kvp.Value.Count)];
                }
            }

            return null;
        }

        public string GetAnotherResponse(string keyword)
        {
            if (keyword != null && _responses.ContainsKey(keyword))
                return _responses[keyword][_random.Next(_responses[keyword].Count)];
            return null;
        }

        public List<string> GetAllKeywords()
        {
            return new List<string>(_responses.Keys);
        }
    }
}
