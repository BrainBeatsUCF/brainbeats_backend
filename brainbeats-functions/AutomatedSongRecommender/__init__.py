import sys
sys.path.append('.')
sys.path.append('..')

import logging
import os
import json
import azure.functions as func
import requests
import time
import datetime

from commons.DatabaseConnection import DatabaseConnection
from commons.KNN import KNN

DATABASE_URL = os.environ['database_url']
DATABASE_NAME = os.environ['database_name']
DATABASE_COLLECTION = os.environ['database_collection']
DATABASE_KEY = os.environ['database_key']
LIMIT = int(os.environ['likes_limit'])
K = int(os.environ['KNN_K'])

db_conn = DatabaseConnection(DATABASE_URL, DATABASE_NAME, DATABASE_COLLECTION, DATABASE_KEY)

async def main(mytimer: func.TimerRequest) -> None:
    utc_timestamp = datetime.datetime.utcnow().replace(
        tzinfo = datetime.timezone.utc).isoformat()

    if mytimer.past_due:
        logging.info(f'Automated Song Recommender is past due: {utc_timestamp}')

    logging.info(f'Automated Song Recommender ran at {utc_timestamp}')

    all_users = await db_conn.execute_query(db_conn.query_map['get_all_users'])
    for user in all_users:
        email = user['id']

        # Get all public beats
        all_beats = await db_conn.execute_query(db_conn.query_map['get_all_public_beats'])

        # Get liked beats
        liked_beats = await db_conn.execute_query(db_conn.query_map['get_liked_beats'].format(email = email, 
            limit = LIMIT))

        try:
            # Get the K Recc'ed Beats
            knn = KNN(K)
            recc_beats = knn.get_k_nearest_neighbors(liked_beats, all_beats)

            current_time = int(time.time())

            # Drop the current recommendations
            await db_conn.execute_query(db_conn.query_map['delete_recommendations'].format(email = email))

            for beat in recc_beats:
                await db_conn.execute_query(db_conn.query_map['recommend_beat'].format(email = email,
                    current_time = current_time, beat_id = beat['id']))
        except Exception as e:
            logging.info(f'Error in KNN: {e}')

        logging.info(f'{email} completed recommendations')
    logging.info('Automated Song Recommender finished')