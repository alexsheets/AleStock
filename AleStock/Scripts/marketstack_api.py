import json
from urllib.request import Request, urlopen
from datetime import date as datemethod
from datetime import timedelta

class MarketStackAPI:
    
    # initialize marketstack class using api key
    def __init__(self, key):
        self.key = key

    
