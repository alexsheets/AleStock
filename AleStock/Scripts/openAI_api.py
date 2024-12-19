﻿import json
from urllib.request import Request, urlopen, urllib


class OpenAI:
    def __init__(self, api_key):
        self.key = api_key
    
        
    def chat(self, messages, model):
        
        req_link = "https://api.openai.com/v1/chat/completions"
        
        data = {
            "model": model,
            "messages": messages,
            "temperature": 0.0,
            "user": "self"
        }
        
        data = urllib.parse.urlencode(data).encode()
        
        req = Request(req_link, data=data)
        req.add_header('Accept', 'application/json, text/plain, */*')
        req.add_header('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36')
        req.add_header('Content-Type', 'application/json')
        req.add_header('Authorization', f'Bearer {self.key}')
        
        content = urlopen(req)
        data = json.load(content)
        
        return data["choices"][0]["message"]["content"]