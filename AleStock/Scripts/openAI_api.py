﻿import json
from urllib.request import Request, urlopen, urllib


class OpenAI:
    def __init__(self, api_key, financial_info):
        self.key = api_key
        self.json_data = financial_info
    
    def create_prompt(self):
        return """
        You will act as a financial analyst. 
        You will be given some financial information for one quarter relating to a particular company in JSON format.
        The financial concepts are passed in the JSON as keys. 
        The values of the JSON are the associated amounts of money the company reported for the quarter.
        Briefly explain each related financial concept and what the amount of money associated means for the company. 
        Analyze the information and give some advice as to whether the company finds itself in good standing.
        You should try to relay the financial information in such a way that it is easily understandable,
        as if it were being written for someone who is a beginner in understanding the stock market.
        """
        
    def chat(self):
        
        req_link = "https://api.openai.com/v1/chat/completions"

        messages = [
            self.create_prompt(),
            self.json_data
        ]
        
        data = {
            "model": "gpt-40-mini",
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