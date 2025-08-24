#!/usr/bin/env python3
from datetime import datetime

# Simulating the VisitQueryParameters.ToDictionary() method
start = 0
limit = 20
scheduled_date_from = datetime(2025, 8, 14)
scheduled_date_to = datetime(2025, 8, 15)

query_dict = {
    "start": str(start),
    "limit": str(limit),
    "scheduled_date_from": scheduled_date_from.strftime("%Y-%m-%d"),
    "scheduled_date_to": scheduled_date_to.strftime("%Y-%m-%d")
}

# Build query string
query_string = "&".join([f"{k}={v}" for k, v in query_dict.items()])

print("For date range search from 2025-08-14 to 2025-08-15:")
print(f"\nGenerated URL: /api/ev1/visits?{query_string}")
print("\nQuery parameters being sent:")
for k, v in query_dict.items():
    print(f"  {k} = {v}")