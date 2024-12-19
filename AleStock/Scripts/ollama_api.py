import json
from urllib.request import Request, urlopen, urllib


class Ollama:
    def __init__(self):
        self.url = 'http://localhost:11434'
    
        
    def embeddings(self, prompt):
        req_link = f"{self.base_url}/api/embeddings"

        data = {
            "prompt": prompt,
            "model": "mxbai-embed-large",
        }

        data = urllib.parse.urlencode(data).encode()
        
        req = Request(req_link, data=data)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        req.add_header('Content-Type', 'application/json')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data["embedding"]
    
        
    def chat(self, messages, model):
        req_link = f"{self.url}/api/chat"
        
        data = {
            "model": model,
            "messages": messages,
            "temperature": 0.0,
            "stream": False,
        }
        
        data = urllib.parse.urlencode(data).encode()
        
        req = Request(req_link, data=data)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        req.add_header('Content-Type', 'application/json')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data["message"]["content"]