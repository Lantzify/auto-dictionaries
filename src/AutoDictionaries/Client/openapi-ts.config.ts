import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
	input: 'https://localhost:44360/umbraco/swagger/autoDictionaries/swagger.json',
	output: {
		path: 'src/api',
	},
	plugins: [
		{
			name: '@hey-api/sdk',
			asClass: true,
			classNameBuilder: '{{name}}Service',
		},
	],
});