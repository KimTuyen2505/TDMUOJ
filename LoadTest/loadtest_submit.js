import http from 'k6/http';

export const options = {
  scenarios: {
    burst: {
      executor: 'shared-iterations',
      vus: 20,           // số VUs
      iterations: 20,    // tổng iterations
      maxDuration: '1m'  // thời gian tối đa (để tránh treo nếu có VU chậm)
    },
  },
};

export default function () {
  http.post('https://tdmuoj.id.vn/Submit', {
    code: 'print("Hello world!")',
    language: '71',
    problemId: '1004'
  });
}
