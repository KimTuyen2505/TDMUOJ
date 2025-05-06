import http from 'k6/http';
import { check } from 'k6';

export const options = {
  scenarios: {
    simultaneous_requests: {
      executor: 'shared-iterations',
      vus: 1000, // số VUs
      iterations: 1000, // số VUs,
      maxDuration: '1m',
    },
  },
};

export default function () {
  const res = http.get('https://tdmuoj.id.vn/Exams/DetailExam/21');
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
}
